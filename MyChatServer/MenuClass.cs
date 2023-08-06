using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MyChatLib;

namespace MyChatServer
{
    internal class MenuItem : IMenuItem
    {
        private readonly Action? _process;
        protected readonly ChatClient _client;
        protected readonly StreamWriter _writer;
        protected readonly StreamReader _reader;
        public string? Title { get; }

        public string? Description { get; } = string.Empty;


        public MenuItem(ChatClient client, string? title, Action? process = null, string? description = null)
        {
            _process = process ?? new Action(() => {  });
            Title = title;
            Description = description;
            _client = client;
            _reader = new StreamReader(_client.TcpClient.GetStream());
        }

        public virtual void Process() => _process();
        
    }
    internal class Menu : MenuItem, IMenu
    {
        private int _index = 0;
        private bool _isExit = false;

        private readonly List<IMenuItem> _items = new();
        public IEnumerable<IMenuItem> Items => _items;

        public Menu(ChatClient client, string? title, Action? process = null, string? description = null)
            : base(client, title, process, description)
        { }

        public void AddMenuItem(IMenuItem item)
        {
            _items.Add(item);
        }

        public override void Process()
        {
            _client.SendMessage($"#Menu#[{_client.TcpClient.Client.RemoteEndPoint}]{Title}");
            _client.SendMessage($"#Menu#[{_client.TcpClient.Client.RemoteEndPoint}]");


            while (!_isExit)
            {
                //_items.OrderBy(o => o.);
                for (int i = 0; i < _items.Count; i++)
                {
                    if (i == _index) _client.SendMessage($"#Menu#[{_client.TcpClient.Client.RemoteEndPoint}]{_items[i].Title} <--");
                    else _client.SendMessage($"#Menu#[{_client.TcpClient.Client.RemoteEndPoint}]{_items[i].Title}");
                }

                MoveEnter();
            }
            _isExit = false;
        }
        private void MoveEnter()
        {
            var key = (ConsoleKey)_reader.ReadLine().ToUpper().FirstOrDefault('b');

            switch (key)
            {
                case ConsoleKey.W: _index = (_index - 1 < 0) ? _items.Count - 1 : _index - 1;
                    break;
                case ConsoleKey.S: _index = (_index + 1 > _items.Count - 1) ? 0 : _index + 1;
                    break;
                case ConsoleKey.D:
                    try
                    {
                        _items[_index].Process();
                    }
                    catch
                    {
                        _index = 0;
                    }
                    break;
                case ConsoleKey.A:
                    _isExit = true;
                    _index = 0;
                    break;
                case ConsoleKey.Backspace: goto case ConsoleKey.A;
            }
        }
        internal static Menu DetectMenu<T>(ChatClient client) where T : new() => DetectMenu(client, new Menu(client, null), typeof(T));
        
        private static Menu DetectMenu(ChatClient client, Menu newMenu, Type typeMenu)
        {

            var obj = Activator.CreateInstance(typeMenu);

            var menuItems = typeMenu.GetMethods()
                .Where(m => m.GetCustomAttribute<MenuActionAttribute>() != null)
                .Select(m =>
                {
                    var attribute = m.GetCustomAttribute<MenuActionAttribute>();
                    return new MenuItem(client, attribute!.Title, () => { m.Invoke(obj, null); });
                });

            var subMenus = typeMenu.GetProperties()
                .Where(p => p.GetCustomAttribute<MenuSubmenuAttribute>() != null)
                .Select(p =>
                {
                    var attribute = p.GetCustomAttribute<MenuSubmenuAttribute>();
                    return new { Menu = new Menu(client, attribute!.Title ?? p.Name), Type = p.PropertyType };
                });

            foreach (var menu in subMenus)
            {
                newMenu.AddMenuItem(DetectMenu(client, menu.Menu, menu.Type));
            }
            foreach (var item in menuItems)
            {
                newMenu.AddMenuItem(item);
            }


            return newMenu;



            //var tcpClient = client.TcpClient;
            //var stream = client.TcpClient.GetStream();
            //var reader = new StreamReader(stream);
            //var writer = new StreamWriter(stream);

            //var Menus = new Menu(client, "Main menu");
            //Menus.AddMenuItem(new MenuItem(client, "Public chat", new Action(async () =>
            //{ 
            //    var task = Task.CompletedTask;
            //    task = Task.Run(() =>
            //    {
            //        string? line = null;
            //        do
            //        {
            //            line = reader.ReadLine();


            //            client.Log(line);
            //            client.MessageReciveUse(line);




            //        } while (!string.IsNullOrEmpty(line));
            //    });
            //    await task;
            //})));
            //Menus.AddMenuItem(new MenuItem(client, "Private chat"));
            //Menus.AddMenuItem(new MenuItem(client, "Rename", new Action(() =>
            //{
            //    writer.Flush();
            //    client.SendMessage("Please Enter your NickName: ", ConsoleColor.DarkYellow);
            //    //_writer.Write("Please Enter your NickName: ");
            //    var nickName = reader.ReadLine();
            //    client.ChangeName(nickName);
            //})));
            //Menus.AddMenuItem(new MenuItem(client, "Disconnect", new Action(() => { client.Dispose();  })));
            //return Menus;
        }
    }
}
