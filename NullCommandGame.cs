using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Forms;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;

namespace NullCommand
{
    // Game settings configuration
    public class GameSettings
    {
        public double SpeedMultiplier { get; set; } = 1.0;
        public bool BossMode { get; set; } = false;
        public double VolumeLevel { get; set; } = 1.0;
    }

    // Unit states for behavior tracking
    public enum UnitState 
    { 
        Guard, Moving, Seeking, Combat, Selected, Dead 
    }

    // Building states
    public enum BuildingState
    {
        Building, Idle, Destroyed
    }

    // Entity types
    public enum EntityType
    {
        Hospital, PlayerSoldier, EnemySoldier
    }

    // Squad formation for units
    public class Squad
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public List<Soldier> Members { get; set; } = new List<Soldier>();
        public Point FormationCenter { get; set; }
        public Point TargetPosition { get; set; }
        public bool InCombat { get; set; } = false;
        public Soldier? Leader { get; set; }
        
        public void AddMember(Soldier soldier)
        {
            if (Members.Count < 5)
            {
                Members.Add(soldier);
                soldier.Squad = this;
                if (Leader == null) Leader = soldier;
            }
        }
        
        public void RemoveMember(Soldier soldier)
        {
            Members.Remove(soldier);
            if (Leader == soldier && Members.Count > 0)
                Leader = Members[0];
            else if (Members.Count == 0)
                Leader = null;
        }
    }

    // Base game entity
    public abstract class GameEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public Point Position { get; set; }
        public double Health { get; set; } = 100;
        public double MaxHealth { get; set; } = 100;
        public EntityType Type { get; set; }
        public Rectangle? Visual { get; set; }
        public bool IsSelected { get; set; } = false;
        public DateTime LastUpdate { get; set; } = DateTime.Now;
    }

    // Hospital building class
    public class Hospital : GameEntity
    {
        public BuildingState State { get; set; } = BuildingState.Building;
        public int BuildingFrame { get; set; } = 0;
        public int IdleFrame { get; set; } = 0;
        public DateTime LastSoldierSpawn { get; set; } = DateTime.MinValue;
        public const double SPAWN_COOLDOWN = 2.0; // seconds
        public const double HEALTH_REGEN_RANGE = 256.0;
        public const double HEALTH_REGEN_AMOUNT = 10.0; // per second
        
        public Hospital()
        {
            Type = EntityType.Hospital;
            Health = 500;
            MaxHealth = 500;
        }
        
        public bool CanSpawnSoldier()
        {
            return State == BuildingState.Idle && 
                   (DateTime.Now - LastSoldierSpawn).TotalSeconds >= SPAWN_COOLDOWN;
        }
        
        public void SpawnSoldier()
        {
            LastSoldierSpawn = DateTime.Now;
        }
    }

    // Soldier unit class
    public class Soldier : GameEntity
    {
        public UnitState State { get; set; } = UnitState.Guard;
        public Point GuardPosition { get; set; }
        public Point TargetPosition { get; set; }
        public Squad? Squad { get; set; }
        public Soldier? Target { get; set; }
        public double MoveSpeed { get; set; } = 2.0;
        public double AttackRange { get; set; } = 80.0;
        public double DetectionRange { get; set; } = 120.0;
        public DateTime LastFired { get; set; } = DateTime.MinValue;
        public const double FIRE_COOLDOWN = 1.0; // seconds
        public const double DAMAGE_PER_SHOT = 15.0;
        public bool IsPlayerControlled { get; set; }
        
        public Soldier(bool isPlayerControlled = true)
        {
            Type = isPlayerControlled ? EntityType.PlayerSoldier : EntityType.EnemySoldier;
            IsPlayerControlled = isPlayerControlled;
            Health = 80;
            MaxHealth = 80;
        }
        
        public bool CanFire()
        {
            return (DateTime.Now - LastFired).TotalSeconds >= FIRE_COOLDOWN;
        }
        
        public void Fire()
        {
            LastFired = DateTime.Now;
        }
        
        public double DistanceTo(GameEntity other)
        {
            return Math.Sqrt(Math.Pow(Position.X - other.Position.X, 2) + 
                           Math.Pow(Position.Y - other.Position.Y, 2));
        }
    }

    public partial class MainWindow : Window
    {
        // Win32 API imports for mouse hooks and keyboard
        private const int WH_MOUSE_LL = 14;
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_KEYDOWN = 0x0100;
        private const int VK_F12 = 0x7B;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        private delegate IntPtr LowLevelProc(int nCode, IntPtr wParam, IntPtr lParam);

        // Class members
        private IntPtr _mouseHookID = IntPtr.Zero;
        private IntPtr _keyboardHookID = IntPtr.Zero;
        private LowLevelProc _mouseProc = MouseHookCallback;
        private LowLevelProc _keyboardProc = KeyboardHookCallback;
        private static MainWindow _instance;
        
        private DispatcherTimer _gameTimer;
        private Canvas _gameCanvas;
        
        private Point _currentMousePos;
        private Point _lastClickPos;
        private bool _shiftPressed = false;
        
        // Game state
        private Hospital? _hospital;
        private List<Soldier> _playerSoldiers = new List<Soldier>();
        private List<Soldier> _enemySoldiers = new List<Soldier>();
        private List<Squad> _playerSquads = new List<Squad>();
        private List<Squad> _enemySquads = new List<Squad>();
        private Soldier? _selectedSoldier;
        private Squad? _selectedSquad;
        
        private Random _random = new Random();
        private DateTime _lastEnemySpawn = DateTime.MinValue;
        private const double ENEMY_SPAWN_COOLDOWN = 1.0; // seconds
        
        // Settings and system tray
        private NotifyIcon _notifyIcon;
        private ContextMenuStrip _contextMenu;
        private GameSettings _settings;
        private string _settingsPath;
        private bool _bossMode = false;

        public MainWindow()
        {
            _instance = this;
            SetupWindow();
            SetupGameCanvas();
            SetupTimers();
            SetupMouseHooks();
            SetupSystemTray();
            LoadSettings();
        }

        private void SetupWindow()
        {
            // Make window transparent and click-through when not in boss mode
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            Topmost = true;
            ShowInTaskbar = false;
            
            // Cover entire virtual screen (all monitors)
            Left = SystemParameters.VirtualScreenLeft;
            Top = SystemParameters.VirtualScreenTop;
            Width = SystemParameters.VirtualScreenWidth;
            Height = SystemParameters.VirtualScreenHeight;
            
            WindowState = WindowState.Normal;
        }

        private void SetupGameCanvas()
        {
            _gameCanvas = new Canvas();
            _gameCanvas.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)); // Transparent
            Content = _gameCanvas;
        }

        private void SetupTimers()
        {
            // Main game timer
            _gameTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
            };
            _gameTimer.Tick += GameTimer_Tick;
            _gameTimer.Start();
        }

        private void SetupMouseHooks()
        {
            _mouseHookID = SetHook(_mouseProc, WH_MOUSE_LL);
            _keyboardHookID = SetHook(_keyboardProc, WH_KEYBOARD_LL);
        }

        private IntPtr SetHook(LowLevelProc proc, int hookType)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(hookType, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                _instance?.OnMouseActivity(wParam);
            }
            return CallNextHookEx(_instance._mouseHookID, nCode, wParam, lParam);
        }

        private static IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                _instance?.OnKeyPress(vkCode);
            }
            return CallNextHookEx(_instance._keyboardHookID, nCode, wParam, lParam);
        }

        private void OnMouseActivity(IntPtr wParam)
        {
            if (_bossMode) return;

            // Get current mouse position
            GetCursorPos(out POINT point);
            _currentMousePos = new Point(point.x, point.y);
            
            // Convert to canvas coordinates
            var canvasPos = new Point(
                _currentMousePos.X - SystemParameters.VirtualScreenLeft,
                _currentMousePos.Y - SystemParameters.VirtualScreenTop
            );

            // Handle click detection
            if (wParam == (IntPtr)WM_LBUTTONDOWN)
            {
                _lastClickPos = canvasPos;
                _shiftPressed = (GetAsyncKeyState(0x10) & 0x8000) != 0; // VK_SHIFT
                
                HandleClick(canvasPos);
            }
        }

        private void OnKeyPress(int vkCode)
        {
            if (vkCode == VK_F12)
            {
                ToggleBossMode();
            }
        }

        private void HandleClick(Point position)
        {
            if (_hospital == null && _shiftPressed)
            {
                // Create hospital at click position
                CreateHospital(position);
            }
            else if (_hospital != null && _shiftPressed)
            {
                // Check if clicking on hospital to spawn soldier
                var distanceToHospital = Math.Sqrt(
                    Math.Pow(position.X - _hospital.Position.X, 2) + 
                    Math.Pow(position.Y - _hospital.Position.Y, 2)
                );
                
                if (distanceToHospital < 64) // Hospital size
                {
                    SpawnSoldier();
                }
                else
                {
                    // Check if clicking on a soldier to select
                    SelectSoldier(position);
                }
            }
            else if (!_shiftPressed && (_selectedSoldier != null || _selectedSquad != null))
            {
                // Give move order
                GiveMoveOrder(position);
            }
            
            // Spawn enemies at opposite edge
            if (_hospital != null)
            {
                SpawnEnemies(position);
            }
        }

        private void CreateHospital(Point position)
        {
            _hospital = new Hospital();
            _hospital.Position = position;
            
            // Create visual representation
            var hospitalRect = new Rectangle
            {
                Width = 64,
                Height = 64,
                Fill = Brushes.Green,
                Stroke = Brushes.DarkGreen,
                StrokeThickness = 2
            };
            
            _hospital.Visual = hospitalRect;
            _gameCanvas.Children.Add(hospitalRect);
            Canvas.SetLeft(hospitalRect, position.X - 32);
            Canvas.SetTop(hospitalRect, position.Y - 32);
        }

        private void SpawnSoldier()
        {
            if (_hospital == null || !_hospital.CanSpawnSoldier()) return;
            
            var soldier = new Soldier(true);
            soldier.Position = new Point(_hospital.Position.X, _hospital.Position.Y + 80);
            soldier.GuardPosition = soldier.Position;
            
            // Create visual representation
            var soldierRect = new Rectangle
            {
                Width = 16,
                Height = 16,
                Fill = Brushes.Red,
                Stroke = Brushes.DarkRed,
                StrokeThickness = 1
            };
            
            soldier.Visual = soldierRect;
            _gameCanvas.Children.Add(soldierRect);
            
            _playerSoldiers.Add(soldier);
            _hospital.SpawnSoldier();
            
            // Assign to squad
            AssignToSquad(soldier);
        }

        private void AssignToSquad(Soldier soldier)
        {
            // Find existing squad with space
            var availableSquad = _playerSquads.FirstOrDefault(s => s.Members.Count < 5);
            
            if (availableSquad != null)
            {
                availableSquad.AddMember(soldier);
            }
            else
            {
                // Create new squad
                var newSquad = new Squad();
                newSquad.AddMember(soldier);
                _playerSquads.Add(newSquad);
            }
        }

        private void SelectSoldier(Point position)
        {
            // Clear previous selection
            if (_selectedSoldier != null && _selectedSoldier.Visual != null)
            {
                _selectedSoldier.Visual.Stroke = Brushes.DarkRed;
                _selectedSoldier.IsSelected = false;
            }
            
            _selectedSoldier = null;
            _selectedSquad = null;
            
            // Find soldier at position
            foreach (var soldier in _playerSoldiers)
            {
                var distance = Math.Sqrt(
                    Math.Pow(position.X - soldier.Position.X, 2) + 
                    Math.Pow(position.Y - soldier.Position.Y, 2)
                );
                
                if (distance < 20)
                {
                    _selectedSoldier = soldier;
                    _selectedSquad = soldier.Squad;
                    soldier.IsSelected = true;
                    
                    if (soldier.Visual != null)
                        soldier.Visual.Stroke = Brushes.Yellow;
                    
                    break;
                }
            }
        }

        private void GiveMoveOrder(Point position)
        {
            if (_selectedSquad != null)
            {
                _selectedSquad.TargetPosition = position;
                foreach (var member in _selectedSquad.Members)
                {
                    member.State = UnitState.Moving;
                    member.TargetPosition = position;
                }
            }
        }

        private void SpawnEnemies(Point lastClickPosition)
        {
            if ((DateTime.Now - _lastEnemySpawn).TotalSeconds < ENEMY_SPAWN_COOLDOWN) return;
            
            _lastEnemySpawn = DateTime.Now;
            
            // Determine spawn edge opposite to click
            Point spawnPoint;
            var screenWidth = SystemParameters.VirtualScreenWidth;
            var screenHeight = SystemParameters.VirtualScreenHeight;
            
            if (lastClickPosition.X < screenWidth / 2)
            {
                // Click on left, spawn on right
                spawnPoint = new Point(screenWidth - 50, _random.NextDouble() * screenHeight);
            }
            else
            {
                // Click on right, spawn on left
                spawnPoint = new Point(50, _random.NextDouble() * screenHeight);
            }
            
            // Spawn group of 3 enemies
            var enemySquad = new Squad();
            for (int i = 0; i < 3; i++)
            {
                var enemy = new Soldier(false);
                enemy.Position = new Point(spawnPoint.X + i * 20, spawnPoint.Y + i * 10);
                enemy.GuardPosition = _hospital?.Position ?? new Point(screenWidth / 2, screenHeight / 2);
                enemy.State = UnitState.Seeking;
                
                // Create visual representation
                var enemyRect = new Rectangle
                {
                    Width = 16,
                    Height = 16,
                    Fill = Brushes.Blue,
                    Stroke = Brushes.DarkBlue,
                    StrokeThickness = 1
                };
                
                enemy.Visual = enemyRect;
                _gameCanvas.Children.Add(enemyRect);
                
                _enemySoldiers.Add(enemy);
                enemySquad.AddMember(enemy);
            }
            
            _enemySquads.Add(enemySquad);
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (_bossMode) return;
            
            UpdateHospital();
            UpdateSoldiers();
            UpdateCombat();
            UpdateHealthRegeneration();
            UpdateVisuals();
        }

        private void UpdateHospital()
        {
            if (_hospital == null) return;
            
            // Update building animation
            if (_hospital.State == BuildingState.Building)
            {
                _hospital.BuildingFrame++;
                if (_hospital.BuildingFrame >= 9 * 8) // 9 frames at 8 ticks per frame
                {
                    _hospital.State = BuildingState.Idle;
                    _hospital.BuildingFrame = 0;
                }
            }
            else if (_hospital.State == BuildingState.Idle)
            {
                _hospital.IdleFrame = (_hospital.IdleFrame + 1) % (4 * 8); // 4 frames at 8 ticks per frame
            }
        }

        private void UpdateSoldiers()
        {
            // Update player soldiers
            foreach (var soldier in _playerSoldiers.ToList())
            {
                UpdateSoldierAI(soldier);
            }
            
            // Update enemy soldiers
            foreach (var enemy in _enemySoldiers.ToList())
            {
                UpdateEnemyAI(enemy);
            }
        }

        private void UpdateSoldierAI(Soldier soldier)
        {
            switch (soldier.State)
            {
                case UnitState.Guard:
                    // Look for enemies
                    var nearbyEnemy = FindNearbyEnemy(soldier);
                    if (nearbyEnemy != null)
                    {
                        soldier.Target = nearbyEnemy;
                        soldier.State = UnitState.Seeking;
                    }
                    else
                    {
                        // Return to guard position
                        MoveTowards(soldier, soldier.GuardPosition, 0.5);
                    }
                    break;
                    
                case UnitState.Moving:
                    MoveTowards(soldier, soldier.TargetPosition, soldier.MoveSpeed);
                    
                    // Check if reached target
                    var distanceToTarget = Math.Sqrt(
                        Math.Pow(soldier.Position.X - soldier.TargetPosition.X, 2) + 
                        Math.Pow(soldier.Position.Y - soldier.TargetPosition.Y, 2)
                    );
                    
                    if (distanceToTarget < 10)
                    {
                        soldier.State = UnitState.Guard;
                        soldier.GuardPosition = soldier.TargetPosition;
                    }
                    break;
                    
                case UnitState.Seeking:
                    if (soldier.Target != null && soldier.Target.Health > 0)
                    {
                        var distanceToEnemy = soldier.DistanceTo(soldier.Target);
                        
                        if (distanceToEnemy <= soldier.AttackRange)
                        {
                            soldier.State = UnitState.Combat;
                        }
                        else
                        {
                            MoveTowards(soldier, soldier.Target.Position, soldier.MoveSpeed);
                        }
                    }
                    else
                    {
                        soldier.State = UnitState.Guard;
                        soldier.Target = null;
                    }
                    break;
                    
                case UnitState.Combat:
                    if (soldier.Target != null && soldier.Target.Health > 0)
                    {
                        var distanceToEnemy = soldier.DistanceTo(soldier.Target);
                        
                        if (distanceToEnemy > soldier.AttackRange)
                        {
                            soldier.State = UnitState.Seeking;
                        }
                        else if (soldier.CanFire())
                        {
                            soldier.Fire();
                            soldier.Target.Health -= Soldier.DAMAGE_PER_SHOT;
                            
                            if (soldier.Target.Health <= 0)
                            {
                                KillSoldier(soldier.Target);
                                soldier.Target = null;
                                soldier.State = UnitState.Guard;
                            }
                        }
                    }
                    else
                    {
                        soldier.State = UnitState.Guard;
                        soldier.Target = null;
                    }
                    break;
            }
        }

        private void UpdateEnemyAI(Soldier enemy)
        {
            switch (enemy.State)
            {
                case UnitState.Seeking:
                    // Move towards hospital
                    if (_hospital != null)
                    {
                        MoveTowards(enemy, _hospital.Position, enemy.MoveSpeed * 0.7); // Slower movement
                        
                        // Look for player soldiers to attack
                        var nearbyPlayer = FindNearbyPlayerSoldier(enemy);
                        if (nearbyPlayer != null)
                        {
                            enemy.Target = nearbyPlayer;
                            enemy.State = UnitState.Combat;
                        }
                    }
                    break;
                    
                case UnitState.Combat:
                    if (enemy.Target != null && enemy.Target.Health > 0)
                    {
                        var distanceToPlayer = enemy.DistanceTo(enemy.Target);
                        
                        if (distanceToPlayer > enemy.AttackRange)
                        {
                            MoveTowards(enemy, enemy.Target.Position, enemy.MoveSpeed);
                        }
                        else if (enemy.CanFire())
                        {
                            enemy.Fire();
                            enemy.Target.Health -= Soldier.DAMAGE_PER_SHOT;
                            
                            if (enemy.Target.Health <= 0)
                            {
                                KillSoldier(enemy.Target);
                                enemy.Target = null;
                                enemy.State = UnitState.Seeking;
                            }
                        }
                    }
                    else
                    {
                        enemy.State = UnitState.Seeking;
                        enemy.Target = null;
                    }
                    break;
            }
        }

        private Soldier? FindNearbyEnemy(Soldier soldier)
        {
            return _enemySoldiers
                .Where(e => e.Health > 0 && soldier.DistanceTo(e) <= soldier.DetectionRange)
                .OrderBy(e => soldier.DistanceTo(e))
                .FirstOrDefault();
        }

        private Soldier? FindNearbyPlayerSoldier(Soldier enemy)
        {
            return _playerSoldiers
                .Where(p => p.Health > 0 && enemy.DistanceTo(p) <= enemy.DetectionRange)
                .OrderBy(p => enemy.DistanceTo(p))
                .FirstOrDefault();
        }

        private void MoveTowards(Soldier soldier, Point target, double speed)
        {
            var deltaX = target.X - soldier.Position.X;
            var deltaY = target.Y - soldier.Position.Y;
            var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
            
            if (distance > 1)
            {
                soldier.Position = new Point(
                    soldier.Position.X + (deltaX / distance) * speed,
                    soldier.Position.Y + (deltaY / distance) * speed
                );
            }
        }

        private void KillSoldier(Soldier soldier)
        {
            if (soldier.Visual != null)
            {
                _gameCanvas.Children.Remove(soldier.Visual);
            }
            
            if (soldier.IsPlayerControlled)
            {
                _playerSoldiers.Remove(soldier);
                soldier.Squad?.RemoveMember(soldier);
            }
            else
            {
                _enemySoldiers.Remove(soldier);
                soldier.Squad?.RemoveMember(soldier);
            }
        }

        private void UpdateCombat()
        {
            // Squad-based combat coordination would be implemented here
            // For now, individual combat is handled in UpdateSoldierAI
        }

        private void UpdateHealthRegeneration()
        {
            if (_hospital == null) return;
            
            foreach (var soldier in _playerSoldiers)
            {
                var distanceToHospital = Math.Sqrt(
                    Math.Pow(soldier.Position.X - _hospital.Position.X, 2) + 
                    Math.Pow(soldier.Position.Y - _hospital.Position.Y, 2)
                );
                
                if (distanceToHospital <= Hospital.HEALTH_REGEN_RANGE)
                {
                    soldier.Health = Math.Min(soldier.MaxHealth, 
                        soldier.Health + Hospital.HEALTH_REGEN_AMOUNT * 0.016); // Per frame
                }
            }
        }

        private void UpdateVisuals()
        {
            // Update hospital visual
            if (_hospital?.Visual != null)
            {
                Canvas.SetLeft(_hospital.Visual, _hospital.Position.X - 32);
                Canvas.SetTop(_hospital.Visual, _hospital.Position.Y - 32);
            }
            
            // Update soldier visuals
            foreach (var soldier in _playerSoldiers.Concat(_enemySoldiers))
            {
                if (soldier.Visual != null)
                {
                    Canvas.SetLeft(soldier.Visual, soldier.Position.X - 8);
                    Canvas.SetTop(soldier.Visual, soldier.Position.Y - 8);
                }
            }
        }

        private void ToggleBossMode()
        {
            _bossMode = !_bossMode;
            _gameCanvas.Visibility = _bossMode ? Visibility.Hidden : Visibility.Visible;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            // Make window click-through when not in boss mode
            var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
        }

        // Additional Win32 API for click-through
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        protected override void OnClosed(EventArgs e)
        {
            // Clean up hooks
            if (_mouseHookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_mouseHookID);
            }
            if (_keyboardHookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_keyboardHookID);
            }
            base.OnClosed(e);
        }

        private void SetupSystemTray()
        {
            // Initialize settings path
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var gameDataPath = System.IO.Path.Combine(appDataPath, "NullCommand");
            Directory.CreateDirectory(gameDataPath);
            _settingsPath = System.IO.Path.Combine(gameDataPath, "settings.json");
            
            // Create system tray icon
            _notifyIcon = new NotifyIcon
            {
                Icon = CreateGameIcon(),
                Text = "Null Command RTS",
                Visible = true
            };
            
            // Create context menu
            CreateContextMenu();
            _notifyIcon.ContextMenuStrip = _contextMenu;
        }
        
        private void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    _settings = JsonSerializer.Deserialize<GameSettings>(json) ?? new GameSettings();
                }
                else
                {
                    _settings = new GameSettings();
                    SaveSettings();
                }
            }
            catch
            {
                _settings = new GameSettings();
            }
        }
        
        private void SaveSettings()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(_settings, options);
                File.WriteAllText(_settingsPath, json);
            }
            catch
            {
                // Settings save failed - continue without saving
            }
        }
        
        private System.Drawing.Icon CreateGameIcon()
        {
            // Create a simple game icon programmatically
            var bitmap = new System.Drawing.Bitmap(16, 16);
            using (var g = System.Drawing.Graphics.FromImage(bitmap))
            {
                g.Clear(System.Drawing.Color.Transparent);
                
                // Draw simple RTS icon shape
                var brush = new System.Drawing.SolidBrush(System.Drawing.Color.DarkGreen);
                
                // Hospital building
                g.FillRectangle(brush, 4, 4, 8, 8);
                
                // Soldiers
                var soldierBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red);
                g.FillRectangle(soldierBrush, 2, 13, 2, 2);
                g.FillRectangle(soldierBrush, 6, 13, 2, 2);
                g.FillRectangle(soldierBrush, 10, 13, 2, 2);
                
                brush.Dispose();
                soldierBrush.Dispose();
            }
            
            return System.Drawing.Icon.FromHandle(bitmap.GetHicon());
        }
        
        private void CreateContextMenu()
        {
            _contextMenu = new ContextMenuStrip();
            
            // Status header
            var statusItem = new ToolStripLabel("⚔️ Null Command RTS")
            {
                Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold)
            };
            _contextMenu.Items.Add(statusItem);
            _contextMenu.Items.Add(new ToolStripSeparator());
            
            // Boss Mode toggle
            var bossModeItem = new ToolStripMenuItem("👁️ Boss Mode (F12)");
            bossModeItem.Click += (s, e) => ToggleBossMode();
            _contextMenu.Items.Add(bossModeItem);
            _contextMenu.Items.Add(new ToolStripSeparator());
            
            // Exit
            var exitItem = new ToolStripMenuItem("❌ Exit");
            exitItem.Click += (s, e) => {
                _notifyIcon.Visible = false;
                System.Windows.Application.Current.Shutdown();
            };
            _contextMenu.Items.Add(exitItem);
        }
    }

    // App.xaml.cs equivalent
    public partial class App : System.Windows.Application
    {
        [STAThread]
        public static void Main()
        {
            var app = new App();
            app.Run(new MainWindow());
        }
    }
}
