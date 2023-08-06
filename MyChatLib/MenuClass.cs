using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

//namespace MyChatServer
//{
//    internal class MenuItem : IMenuItem
//    {
//        private readonly Func<bool>? _process;
//        protected readonly ChatClient _client;
//        protected readonly StreamWriter _writer;
//        protected readonly StreamReader _reader;
//        public string Title { get; }

//        public string? Description { get; } = string.Empty;

//        public int Number { get; }

//        public MenuItem(ChatClient client ,string title, int number, Func<bool> process = null, string description = null)
//        {
//            _process = process ?? new Func<bool>(() => { return true; });
//            Title = title;
//            Description = description;
//            Number = number;
//            _client = client;
//            _reader = new StreamReader(_client.TcpClient.GetStream());
//        }

//        public virtual void Process() => _process();

//    }
//    internal class Menu : MenuItem, IMenu
//    {
//        private int _index = 0;
//        private bool _isExit = false;

//        private readonly List<IMenuItem> _items = new();
//        public IEnumerable<IMenuItem> Items => _items;

//        public Menu(ChatClient client, string title, int number, Func<bool> process = null, string description = null) 
//            : base(client, title, number, process, description) 
//        { }

//        public void AddMenuItem(IMenuItem item)
//        {
//            _items.Add(item);
//        }

//        public override void Process()
//        {
//            _client.SendMessage($"#Menu#[{_client.TcpClient.Client.RemoteEndPoint}]{Title}");
//            _client.SendMessage($"#Menu#[{_client.TcpClient.Client.RemoteEndPoint}]");


//            while (!_isExit)
//            {
//                for (int i = 0; i < _items.Count; i++)
//                {
//                    if (i == _index) _client.SendMessage($"#Menu#[{_client.TcpClient.Client.RemoteEndPoint}]{_items[i].Title} <--");
//                    else _client.SendMessage($"#Menu#[{_client.TcpClient.Client.RemoteEndPoint}]{_items[i].Title}");
//                }

//                MoveEnter();
//            }
//            _isExit = false;
//        }
//        private void MoveEnter()
//        {
//            var key = (ConsoleKey)_reader.ReadLine().ToUpper().FirstOrDefault('b');

//            switch (key)
//            {
//                case ConsoleKey.W:  _index = (_index - 1 < 0) ? _items.Count - 1 : _index - 1;
//                    break;
//                case ConsoleKey.S:  _index = (_index + 1 > _items.Count - 1) ? 0 : _index + 1; 
//                    break;
//                case ConsoleKey.D:
//                    try
//                    {
//                        _items[_index].Process();
//                    }
//                    catch
//                    {
//                        _index = 0;
//                    }
//                    break;
//                case ConsoleKey.A:
//                    _isExit = true;
//                    _index = 0;
//                    break;
//                case ConsoleKey.Backspace: goto case ConsoleKey.A;
//            }
//        }
//    }
//}
