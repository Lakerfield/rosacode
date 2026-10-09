using Lakerfield.RosaCode;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Lakerfield.Rpc;

namespace RpcDemo.Playground
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();

      DataContext = new MainViewModel()
      {
        Code = """
               using System;

               public class Test()
               {
                 public void Execute()
                 {
                   Console.WriteLine("Hello");
                 }
               }
               """
      };

      var engine = new RpcRosaCodeEngineClient(new NetworkClient(new Uri("ws://localhost:5000/ws")));

      _ = editor.InitializeEditor(engine);
    }

    private void ToggleClick(object sender, RoutedEventArgs e)
    {
      if (editor.Mode == RosaCodeMode.Normal)
        editor.Mode = RosaCodeMode.Diff;
      else
        editor.Mode = RosaCodeMode.Normal;
    }

    protected override void OnClosed(EventArgs e)
    {
      _ = editor.CleanupEditor();
      base.OnClosed(e);
    }
  }
}
