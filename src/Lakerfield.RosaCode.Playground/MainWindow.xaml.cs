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

namespace Lakerfield.RosaCode.Playground
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();

      DataContext = new MainViewModel();

      var engine = new RosaCodeRoslynEngine("test", """
        using System;

        public class Test()
        {
          public void Execute()
          {
            Console.WriteLine("Hello");
          }
        }
        """);
      
      _ = editor.InitializeEditor(engine);
    }

    private void ToggleClick(object sender, RoutedEventArgs e)
    {
      if (editor.Mode == RosaCodeMode.Normal)
        editor.Mode = RosaCodeMode.Diff;
      else
        editor.Mode = RosaCodeMode.Normal;
    }

    private async void GetCodeClick(object sender, RoutedEventArgs e)
    {
      MessageBox.Show(await editor.GetCode());
    }

    private void SetCodeClick(object sender, RoutedEventArgs e)
    {
      editor.SetCode("""
        using System;

        public class MyNewClass()
        {
          public void Say()
          {
            Console.WriteLine("Hi!");
          }
        }
        """);
    }
  }
}
