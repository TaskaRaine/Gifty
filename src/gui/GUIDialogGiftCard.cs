using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace Gifty.GUI
{
    class GUIDialogGiftCard : GuiDialog
    {
        protected const int MAX_LINES = 20;
        protected const int MAX_WIDTH = 400;

        public bool IsSigned { get; private set; } = false;
        public string Recipient { get; private set; } = string.Empty;
        public string Message { get; private set; } = string.Empty;

        protected CairoFont Font { get; } = CairoFont.TextInput().WithFontSize(18);

        public GUIDialogGiftCard(ICoreClientAPI capi) : base(capi)
        {
            Compose();
        }
        public GUIDialogGiftCard(ICoreClientAPI capi, string recipient, string message): base(capi)
        {
            IsSigned = true;

            Recipient = recipient; 
            Message = message;

            Compose();
        }

        public override string ToggleKeyCombinationCode
        {
            get { return null; }
        }

        protected virtual void Compose()
        {
            double lineHeight = Font.GetFontExtents().Height * Font.LineHeightMultiplier / RuntimeEnv.GUIScale;
            ElementBounds addressRecipientInfoBounds = ElementBounds.Fixed(0, 0, MAX_WIDTH, 1 * lineHeight + 1);
            ElementBounds recipientBounds = ElementBounds.Fixed(0, 1 * lineHeight + 5, MAX_WIDTH, 1 * lineHeight + 1);
            ElementBounds messageInfoBounds = ElementBounds.Fixed(0, 2 * lineHeight + 10, MAX_WIDTH, 1 * lineHeight + 1);
            ElementBounds textAreaBounds = ElementBounds.Fixed(0, 3 * lineHeight + 15, MAX_WIDTH, 6 * lineHeight + 1);

            ElementBounds closeButtonBounds = ElementBounds.FixedSize(60, 30).FixedUnder(textAreaBounds, 18 + 5).WithAlignment(EnumDialogArea.LeftFixed).WithFixedPadding(10, 2);
            ElementBounds saveButtonBounds = ElementBounds.FixedSize(60, 30).FixedUnder(textAreaBounds, 18 + 5).WithAlignment(EnumDialogArea.RightFixed).WithFixedPadding(10, 2);

            // 2. Around all that is 10 pixel padding
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;

            // 3. Finally Dialog
            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog
                .WithAlignment(EnumDialogArea.CenterMiddle)
                .WithFixedAlignmentOffset(-GuiStyle.DialogToScreenPadding, 0);

            SingleComposer = capi.Gui
                .CreateCompo("blockentitytexteditordialog", dialogBounds)
                .AddShadedDialogBG(bgBounds, false, 5, 0.75f)
                .BeginChildElements(bgBounds)
                .AddIf(IsSigned == false)
                    .AddRichtext(Lang.Get("gifty:guiinfo-address-recipient"), Font, addressRecipientInfoBounds, "addressRecipientInfoBox")
                    .AddTextInput(recipientBounds, null, Font, "recipientBox")
                    .AddRichtext(Lang.Get("gifty:guiinfo-write-message"), Font, messageInfoBounds, "messageInfoBox")
                    .AddTextArea(textAreaBounds, null, Font, "messageBox")

                    .AddSmallButton(Lang.Get("Sign Card"), OnSignedPressed, saveButtonBounds)
                .EndIf()
                .AddIf(IsSigned == true)
                    .AddRichtext(Recipient, Font, recipientBounds, "recipientBox")
                    .AddRichtext(Message, Font, textAreaBounds, "messageBox")
                .EndIf()
                    .AddSmallButton(Lang.Get("Close"), () => TryClose(), closeButtonBounds)

                .EndChildElements()
                .Compose();
        }
        public void SetGiftCardData(string recipient, string message)
        {
            Recipient = recipient;
            Message = message;

            IsSigned = true;
        }
        private bool OnSignedPressed()
        {
            Recipient = SingleComposer.GetTextInput("recipientBox").GetText();
            Message = SingleComposer.GetTextArea("messageBox").GetText();

            IsSigned = true;
            TryClose();

            return IsSigned;
        }
    }
}
