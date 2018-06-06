using System.Windows.Forms;

namespace PerfectChess
{
    public partial class StatsForm : Form
    {
        public StatsForm()
        {
            InitializeComponent();
        }

        public void Reset()
        {
            Box.Text = "";
        }

        public void ShowStats(string text)
        {
            Box.Text += text;
        }
    }
}
