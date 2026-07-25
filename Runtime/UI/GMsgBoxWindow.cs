
using FairyGUI;

namespace VeryFS.Framework.Runtime.UI
{
    public enum MsgBoxStyle
    {
        OkOnly = 0,
        OkCancel,
        YesNo,
        RetryCancel,
        YesNoCancel
    }


    internal class GMsgBoxWindow : GPopupWindow
    {
        public GButton cancelButton;
        private GButton okButton;
        private GButton yesButton;
        private GButton noButton;
        private GButton retryButton;

        public string prompt
        {
            get => _promptTextField.text;
            set => _promptTextField.text = value;
        }

        private Controller _ctrl;
        public MsgBoxStyle style
        {
            get => (MsgBoxStyle)_ctrl.selectedIndex;
            set => _ctrl.selectedIndex = (int)value;
        }

        private GTextField _promptTextField;

        public string title
        {
            get => frame.asLabel.title;
            set => frame.asLabel.title = value;
        }


        public GMsgBoxWindow(UIPackage package, string resName)
            : base(package, resName)
        {

        }

        protected override void OnInit()
        {
            base.OnInit();
            result = PopupResult.Cancel;

            //  var loader = this.frame.GetChild("bg").asImage;
            // Debug.Assert(loader!=null ,"loader!=null");
            // BlurFilter blur = new BlurFilter();
            // blur.blurSize =0.5f;   //设置模糊程度
            // loader.filter = blur;

            var comCancel = this.contentPane.GetChild("cancelButton");
            if (comCancel != null)
            {
                cancelButton = comCancel.asButton;
                cancelButton.onClick.Add(Hide);
            }
            else if (closeButton != null)
            {
                cancelButton = closeButton.asButton;
            }

            _promptTextField = this.contentPane.GetChild("prompt").asTextField;
            _ctrl = this.contentPane.GetController("style");
            style = MsgBoxStyle.OkOnly;

            BindButtonAction(ref okButton, "okButton", PopupResult.Ok);
            BindButtonAction(ref yesButton, "yesButton", PopupResult.Yes);
            BindButtonAction(ref noButton, "noButton", PopupResult.No);
            BindButtonAction(ref retryButton, "retryButton", PopupResult.Retry);
            this.modal = true;
            draggable = true;
        }

        private void BindButtonAction(ref GButton button, string name, PopupResult ret)
        {
            button = null;
            var okCom = this.contentPane.GetChild(name);
            if (okCom != null)
            {
                button = okCom.asButton;
                button.onClick.Add(() =>
                {
                    result = ret;
                    Hide();
                });
            }
        }


        public override void ShowModal(bool popupMode = false)
        {
            this.modal = true;
            this.Center();
            if (popupMode)
            {
                GRoot.inst.ShowPopup(this);
            }
            else
            {
                this.Show();
            }
        }

        protected override void OnHide()
        {

        }
    }


}