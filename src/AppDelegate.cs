using System;
using System.IO;
using System.Diagnostics;
using Foundation;
using AppKit;
using Security;

namespace GoogleImeSkkXamMac
{
    public partial class AppDelegate : NSApplicationDelegate
    {
        NSStatusItem _statusItem;
        NSMenuItem _quitItem;
        NSMenuItem _startItem;
        NSMenuItem _stopItem;

        Process _skkProcess;

        public AppDelegate()
        {
        }

        public override void DidFinishLaunching(NSNotification notification)
        {
            // Dockアイコンを非表示にする
            var app = NSApplication.SharedApplication;
            app.ActivationPolicy = NSApplicationActivationPolicy.Accessory;

            // インストールされているか確認する
            if (!Install())
            {
                //終了
                app.Terminate(this);
            }

            // ステータスバーのメニュー準備
            var appMenu = new NSMenu();
            _quitItem = new NSMenuItem("Quit", "q", 
                (s, e) => NSApplication.SharedApplication.Terminate(this)
            );
            _startItem = new NSMenuItem("Start", "s",
                (s, e) => Start()
            );
            _stopItem = new NSMenuItem("Stop", "k",
                (s, e) => Stop()
            );
           appMenu.AddItem(_quitItem);
            appMenu.AddItem(_startItem);
            appMenu.AddItem(_stopItem);

            // ステータスバー生成
            _statusItem = NSStatusBar.SystemStatusBar.CreateStatusItem(NSStatusItemLength.Variable);
            _statusItem.Title = "あ";
            _statusItem.Menu = appMenu;

            // 実行
            Start();
        }

        public override void WillTerminate(NSNotification notification)
        {
            Stop();
        }

        public bool Install()
        {
            // インストール済み？
            if (File.Exists("/usr/bin/google-ime-skk"))
            {//インストール済み
                return true;
            }

            // インストール（管理者権限で実行する）
            using (var auth = Authorization.Create(AuthorizationFlags.Defaults))
            {
                var args = new[] { "-c", "\"\"gem install google-ime-skk\"\"" };
                var ret = (AuthorizationStatus)auth.ExecuteWithPrivileges("/bin/sh", AuthorizationFlags.Defaults, args);
                if (ret == AuthorizationStatus.Success)
                {
                    return true;
                }
            }

            return false;
        }

        public void Start()
        {
            if (_skkProcess != null)
            {
                return;
            }

            _skkProcess = Process.Start("/usr/bin/google-ime-skk");

            _startItem.Hidden = true;
            _stopItem.Hidden = false;
        }

        public void Stop()
        {
            if (_skkProcess == null)
            {
                return;
            }

            _skkProcess.Kill();
            _skkProcess = null;

            _startItem.Hidden = false;
            _stopItem.Hidden = true;
        }
    }
}

