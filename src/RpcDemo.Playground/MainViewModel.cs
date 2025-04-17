using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpcDemo.Playground
{
  public class MainViewModel : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _code;
    public string Code
    {
      get
      {
        return _code ?? "";
      }
      set
      {
        if (_code == value)
          return;

        _code = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Code)));
      }

    }
  }
}
