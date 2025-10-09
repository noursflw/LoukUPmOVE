using UraniumUI.Material.Controls;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Linq;
using loukupm.ViewModel;  

namespace loukupm.View
{
    public partial class Verificationpage : ContentPage
    {
        bool _suppress;
        Color _defaultBg;

        public Verificationpage()
        {
            InitializeComponent();
            Shell.SetNavBarIsVisible(this, false);
            this.BindingContext = AppViewModel.Instance;
        }
        

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _defaultBg = Digit1.BackgroundColor; // Õ›Ÿ «··Ê‰ «·«› —«÷Ì
        }

        private void Digit_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_suppress) return;

            var fields = new[] { Digit1, Digit2, Digit3, Digit4 };
            var current = (TextField)sender;
            int index = System.Array.IndexOf(fields, current);
            if (index == -1) return;

            // ≈⁄«œ… «··Ê‰ «·ÿ»Ì⁄Ì ⁄‰œ «·ﬂ «»…
            if (!string.IsNullOrEmpty(e.NewTextValue))
                current.BackgroundColor = _defaultBg;

            // «·«‰ ﬁ«· «· ·ﬁ«∆Ì ⁄‰œ «·≈œŒ«·
            if (!string.IsNullOrEmpty(e.NewTextValue) && e.NewTextValue.Length == 1)
            {
                if (index < fields.Length - 1)
                    fields[index + 1].Focus();
                else
                    current.Unfocus();
                return;
            }

            // «·”„«Õ »«·Õ–› «·ÿ»Ì⁄Ì ·ﬂ· Œ«‰…
            // ·« ‰Õ «Ã √Ì „‰ÿﬁ ·„‰⁄ «·Õ–›° √Ì Œ«‰… Ì„ﬂ‰ „”ÕÂ«
        }


        private int LastFilledIndex(TextField[] fields)
        {
            for (int i = fields.Length - 1; i >= 0; i--)
                if (!string.IsNullOrWhiteSpace(fields[i].Text))
                    return i;
            return -1;
        }

        private void HighlightEmptyFields()
        {
            var fields = new[] { Digit1, Digit2, Digit3, Digit4 };
            foreach (var f in fields)
                f.BackgroundColor = string.IsNullOrWhiteSpace(f.Text) ? Colors.Red : _defaultBg;
        }

        private void ConfirmCode_Clicked(object sender, System.EventArgs e)
        {
            var fields = new[] { Digit1, Digit2, Digit3, Digit4 };
            if (fields.Any(f => string.IsNullOrWhiteSpace(f.Text)))
            {
                HighlightEmptyFields();
                return;
            }

            string code = string.Concat(fields.Select(f => f.Text));

            //  «»⁄  ‰›Ì– «·ﬂÊœ Â‰« („À·« ≈—”«· «·ﬂÊœ)
        }
    }
}
