using HandyControl.Controls;
using SmartFillMonitor.Models;
using SmartFillMonitor.Services.Logs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SmartFillMonitor.Services
{
    public class UserService
    {
        //用于密码加密的静态盐
        private const string StaticSalt = "SmartFillMonitor_2026!@#";
        public static event Action<User> LoginStateChanged;
        private static User? _currentUser;
        public static User? CurrentUser
        {
            get { return _currentUser; }
            private set
            {
                if (_currentUser != value)
                {
                    _currentUser = value;
                    LoginStateChanged?.Invoke(_currentUser);
                }
            }
        }

        public static async Task InitializaAsync()
        {
            try
            {
                bool hasUsers = await DbServices.Fsql.Select<User>().AnyAsync();
                if (!hasUsers)
                {
                    var now = DateTime.Now;
                    var users = new List<User>()
                    {
                        new User
                        {
                            UserName="admin",
                            PassWordHash=HashPassword("admin"),
                            Role=Role.Admin,
                            CreateAt=now,
                        },
                        new User
                        {
                            UserName="engineer",
                            PassWordHash=HashPassword("engineer"),
                            Role=Role.Engineer,
                            CreateAt=now,
                        },
                    };

                    var affrows = await DbServices.Fsql.Insert<User>(users).ExecuteAffrowsAsync();
                    if (affrows == 2)
                    {
                        LogServices.Info("系统初始化：创建默认用户成功");
                    }
                    else
                    {
                        throw new Exception("创建失败");
                    }
                }

            }
            catch (Exception ex)
            {
                LogServices.Error("系统初始化失败", ex);
            }
        }

        public static async Task<bool> AutenticateAsync(string userName, string passWord)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(passWord)) return false;
            try
            {
                var user = await DbServices.Fsql.Select<User>()
                    .Where(u => u.UserName == userName)
                    .FirstAsync();

                if (user == null) return false;
                var inputHash = HashPassword(passWord);
                bool isValid = string.Equals(inputHash, user.PassWordHash, StringComparison.Ordinal);
                if (isValid)
                {
                    CurrentUser = user;
                    LogServices.Info($"用户登录成功：{userName}");
                }
                else
                {
                    LogServices.Warn($"用户{userName}尝试登录失败，密码错误");
                }
                return isValid;
            }
            catch (Exception ex)
            {
                LogServices.Error("用户登录验证失败", ex);
                return false;
            }
        }

        //登出
        public static Task LoginOutAsync()
        {
            if (CurrentUser != null)
            {
                LogServices.Info($"用户{CurrentUser.UserName}登出");
                CurrentUser = null;
            }
            return Task.CompletedTask;
        }

        public static async Task CreateUserAsync(string userName, string passWord, Role role, string displayName = "")
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(passWord)) throw new ArgumentNullException("用户名和密码不能为空！");
            bool exists = await DbServices.Fsql.Select<User>()
                .Where(u => u.UserName == userName)
                .AnyAsync();

            if (exists) throw new InvalidCastException($"用户{userName}已经存在");

            var user = new User()
            {
                UserName = userName,
                PassWordHash = HashPassword(passWord),
                Role = role,
                CreateAt = DateTime.Now,
            };

            await DbServices.Fsql.Insert<User>().ExecuteAffrowsAsync();
            LogServices.Info($"创建新用户：{userName}");
        }

        public static async Task<List<User>> GetAllUserAsync()
        {
            try
            {
                return await DbServices.Fsql.Select<User>()
                    .OrderBy(u => u.UserName)
                    .ToListAsync();

            }
            catch (Exception ex)
            {
                LogServices.Error("获取用户列表失败", ex);
                throw;
            }
        }

        private static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return string.Empty;

            string raw = password + StaticSalt;
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));

            var sb = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("X2"));
            }

            return sb.ToString();
        }


    }
}
