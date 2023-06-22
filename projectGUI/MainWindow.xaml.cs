using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace projectGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void GoTo_Info(object sender, RoutedEventArgs e)
        {
            Info i = new Info();
            i.Show();
            this.Close();
        }
        private void GoTo_Sniff(object sender, RoutedEventArgs e)
        {
            login i = new login();
            i.Show();
            this.Close();
        }
        /*private void key_down(object sender, KeyEventArgs e)
{
   if (e.Key == Key.Enter)
   {

   }
}*/


    }

}
/* <TextBlock Foreground="Yellow" x:Name="my_text">
            <TextBlock.Background>
                <SolidColorBrush Color="Orange" Opacity="0.4" />
            </TextBlock.Background>hello</TextBlock>*/
/*<TextBox  x:Name="raz" KeyDown="key_down"></TextBox>*/
