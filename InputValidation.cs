using System.ComponentModel;
using System.Globalization;
using System.Media;
using System.Windows.Forms;

namespace SaldoGo
{
    internal static class InputValidation
    {
        public static void AttachDecimalOnly(TextBox textBox, string fieldLabel)
        {
            if (textBox == null) return;

            textBox.KeyPress -= DecimalOnly_KeyPress;
            textBox.KeyPress += DecimalOnly_KeyPress;

            textBox.Validating -= DecimalOnly_Validating;
            textBox.Validating += DecimalOnly_Validating;

            textBox.Tag = fieldLabel;
        }

        private static void DecimalOnly_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(sender is TextBox tb)) return;

            if (char.IsControl(e.KeyChar)) return;

            char decSep = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];
            char altSep = decSep == '.' ? ',' : '.';

            if (char.IsDigit(e.KeyChar)) return;

            if ((e.KeyChar == decSep || e.KeyChar == altSep) && !tb.Text.Contains(decSep.ToString()) && !tb.Text.Contains(altSep.ToString()))
                return;

            SystemSounds.Beep.Play();
            e.Handled = true;
        }

        private static void DecimalOnly_Validating(object sender, CancelEventArgs e)
        {
            if (!(sender is TextBox tb)) return;

            string label = tb.Tag as string;
            if (string.IsNullOrWhiteSpace(label)) label = "Kolom";

            string text = tb.Text?.Trim() ?? "";
            if (text == "") return;

            decimal tmp;
            if (!decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out tmp))
            {
                MessageBox.Show(label + " hanya boleh diisi angka.");
                tb.SelectAll();
                e.Cancel = true;
            }
        }
    }
}
