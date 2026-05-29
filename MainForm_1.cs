using System;
using System.Drawing;
using System.Media;
using System.IO;
using System.Windows.Forms;

namespace CyberChatbot
{
    public partial class MainForm : Form
    {
        // ── Engine & state ────────────────────────
        private readonly ResponseEngine _engine = new ResponseEngine();
        private bool _soundEnabled = true;

        // ── Colour palette ────────────────────────
        private readonly Color _bgDark       = Color.FromArgb(13,  17,  23);
        private readonly Color _bgPanel      = Color.FromArgb(22,  27,  34);
        private readonly Color _bgInput      = Color.FromArgb(30,  37,  47);
        private readonly Color _accentGreen  = Color.FromArgb(56, 211, 159);
        private readonly Color _accentBlue   = Color.FromArgb(88, 166, 255);
        private readonly Color _textLight    = Color.FromArgb(230, 237, 243);
        private readonly Color _textMuted    = Color.FromArgb(125, 133, 144);
        private readonly Color _userBubble   = Color.FromArgb(35,  81,  138);
        private readonly Color _botBubble    = Color.FromArgb(30,  38,  52);
        private readonly Color _warnColour   = Color.FromArgb(210, 153, 34);
        private readonly Color _infoColour   = Color.FromArgb(88,  166, 255);

        // ── Controls ──────────────────────────────
        private RichTextBox   _chatBox;
        private TextBox       _inputBox;
        private Button        _sendBtn;
        private Button        _clearBtn;
        private Button        _soundBtn;
        private Label         _titleLabel;
        private Label         _statusLabel;
        private Panel         _headerPanel;
        private Panel         _footerPanel;
        private Label         _asciiLabel;

        public MainForm()
        {
            InitialiseComponents();
            ShowWelcome();
        }

        // ─────────────────────────────────────────
        //  Build all controls programmatically
        // ─────────────────────────────────────────
        private void InitialiseComponents()
        {
            // ── Form ─────────────────────────────
            Text            = "🔒 CyberSec Awareness Chatbot";
            Size            = new Size(900, 700);
            MinimumSize     = new Size(700, 560);
            BackColor       = _bgDark;
            ForeColor       = _textLight;
            Font            = new Font("Segoe UI", 10f);
            StartPosition   = FormStartPosition.CenterScreen;

            // ── Header panel ─────────────────────
            _headerPanel = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 110,
                BackColor = _bgPanel,
                Padding   = new Padding(16, 8, 16, 8)
            };

            _asciiLabel = new Label
            {
                Text      = GetAsciiArt(),
                Font      = new Font("Courier New", 7f),
                ForeColor = _accentGreen,
                AutoSize  = false,
                Size      = new Size(300, 90),
                Location  = new Point(16, 10),
                TextAlign = ContentAlignment.MiddleLeft
            };

            _titleLabel = new Label
            {
                Text      = "CyberSec Awareness Chatbot",
                Font      = new Font("Segoe UI Semibold", 16f, FontStyle.Bold),
                ForeColor = _accentGreen,
                AutoSize  = true,
                Location  = new Point(330, 18)
            };

            var subLabel = new Label
            {
                Text      = "Your personal guide to staying safe online 🛡️",
                Font      = new Font("Segoe UI", 10f),
                ForeColor = _textMuted,
                AutoSize  = true,
                Location  = new Point(330, 52)
            };

            _soundBtn = CreateIconButton("🔊", new Point(830, 10), 44, ToggleSound_Click);

            _headerPanel.Controls.AddRange(new Control[] { _asciiLabel, _titleLabel, subLabel, _soundBtn });

            // ── Chat display ─────────────────────
            _chatBox = new RichTextBox
            {
                Dock          = DockStyle.Fill,
                BackColor     = _bgDark,
                ForeColor     = _textLight,
                Font          = new Font("Segoe UI", 10.5f),
                ReadOnly      = true,
                BorderStyle   = BorderStyle.None,
                ScrollBars    = RichTextBoxScrollBars.Vertical,
                Padding       = new Padding(12),
                WordWrap      = true
            };

            var chatPanel = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = _bgDark,
                Padding   = new Padding(10, 6, 10, 6)
            };
            chatPanel.Controls.Add(_chatBox);

            // ── Footer / input area ───────────────
            _footerPanel = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 80,
                BackColor = _bgPanel,
                Padding   = new Padding(12, 10, 12, 10)
            };

            _statusLabel = new Label
            {
                Text      = "💡 Try asking: \"Tell me about phishing\"",
                ForeColor = _textMuted,
                Font      = new Font("Segoe UI", 8.5f),
                AutoSize  = true,
                Location  = new Point(14, 0)
            };

            _inputBox = new TextBox
            {
                Location       = new Point(14, 20),
                Size           = new Size(720, 38),
                BackColor      = _bgInput,
                ForeColor      = _textLight,
                Font           = new Font("Segoe UI", 11f),
                BorderStyle    = BorderStyle.FixedSingle,
                PlaceholderText = "Type your message here…"
            };
            _inputBox.KeyDown += InputBox_KeyDown;

            _sendBtn = new Button
            {
                Text      = "Send  ➤",
                Location  = new Point(744, 20),
                Size      = new Size(90, 38),
                BackColor = _accentGreen,
                ForeColor = _bgDark,
                Font      = new Font("Segoe UI Semibold", 10f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand
            };
            _sendBtn.FlatAppearance.BorderSize = 0;
            _sendBtn.Click += SendBtn_Click;

            _clearBtn = CreateIconButton("🗑 Clear", new Point(843, 20), 86, ClearBtn_Click);

            _footerPanel.Controls.AddRange(new Control[] { _statusLabel, _inputBox, _sendBtn, _clearBtn });

            // ── Quick-topic buttons ───────────────
            var topicsPanel = new FlowLayoutPanel
            {
                Dock        = DockStyle.Bottom,
                Height      = 40,
                BackColor   = _bgPanel,
                Padding     = new Padding(8, 4, 8, 0),
                FlowDirection = FlowDirection.LeftToRight
            };

            string[] topics = { "🔑 Passwords", "🎣 Phishing", "🚨 Scams", "🔒 Privacy", "🦠 Malware", "📶 Wi-Fi", "💰 Ransomware" };
            foreach (var topic in topics)
            {
                var btn = new Button
                {
                    Text      = topic,
                    AutoSize  = true,
                    Height    = 28,
                    BackColor = _bgInput,
                    ForeColor = _accentBlue,
                    Font      = new Font("Segoe UI", 8.5f),
                    FlatStyle = FlatStyle.Flat,
                    Margin    = new Padding(2, 0, 2, 0),
                    Cursor    = Cursors.Hand
                };
                btn.FlatAppearance.BorderColor = _accentBlue;
                btn.FlatAppearance.BorderSize  = 1;
                string t = topic; // capture
                btn.Click += (s, e) => { _inputBox.Text = t.Substring(2).Trim(); SendMessage(); };
                topicsPanel.Controls.Add(btn);
            }

            // ── Layout ────────────────────────────
            Controls.Add(chatPanel);
            Controls.Add(topicsPanel);
            Controls.Add(_footerPanel);
            Controls.Add(_headerPanel);

            // Force resize so footer doesn't overlap chatPanel
            Resize += (s, e) => _inputBox.Width = _footerPanel.Width - 200;
        }

        // ─────────────────────────────────────────
        //  Event handlers
        // ─────────────────────────────────────────
        private void SendBtn_Click(object sender, EventArgs e) => SendMessage();
        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                SendMessage();
            }
        }
        private void ClearBtn_Click(object sender, EventArgs e)
        {
            _chatBox.Clear();
            ShowWelcome();
        }
        private void ToggleSound_Click(object sender, EventArgs e)
        {
            _soundEnabled = !_soundEnabled;
            _soundBtn.Text = _soundEnabled ? "🔊" : "🔇";
        }

        // ─────────────────────────────────────────
        //  Core send / display logic
        // ─────────────────────────────────────────
        private void SendMessage()
        {
            string userText = _inputBox.Text.Trim();
            if (string.IsNullOrEmpty(userText)) return;

            AppendUserMessage(userText);
            _inputBox.Clear();

            var response = _engine.ProcessInput(userText);
            AppendBotMessage(response);
            UpdateStatus(response.Sentiment);

            if (_soundEnabled) PlayTick();
        }

        private void AppendUserMessage(string text)
        {
            AppendLine("");
            AppendColoured("  You  ", _bgDark, _accentBlue, isBold: true);
            AppendColoured($"  {text}\n", _bgDark, _textLight);
        }

        private void AppendBotMessage(ChatResponse response)
        {
            string icon = response.Sentiment switch
            {
                "worried"    => "😟",
                "curious"    => "🤓",
                "frustrated" => "😤",
                _            => "🤖"
            };

            AppendColoured($" {icon} Bot ", _bgDark, _accentGreen, isBold: true);
            AppendColoured($"  {response.Message}\n", _bgDark, _textLight);
            if (response.WasKeywordMatch)
                AppendColoured("        💡 Tip: type \"tell me more\" for another tip on this topic.\n", _bgDark, _textMuted);
        }

        private void ShowWelcome()
        {
            AppendColoured(
                "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n",
                _bgDark, _accentGreen);
            AppendColoured(
                " 🔒 Welcome to the Cybersecurity Awareness Chatbot!\n",
                _bgDark, _accentGreen, isBold: true);
            AppendColoured(
                " Ask me about passwords, phishing, scams, privacy, malware, Wi-Fi\n" +
                " safety, ransomware, and more. You can also tell me your name and\n" +
                " what topics interest you — I'll remember!\n",
                _bgDark, _textMuted);
            AppendColoured(
                "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n",
                _bgDark, _accentGreen);
        }

        // ─────────────────────────────────────────
        //  RichTextBox helpers
        // ─────────────────────────────────────────
        private void AppendColoured(string text, Color bg, Color fg, bool isBold = false)
        {
            _chatBox.SelectionStart  = _chatBox.TextLength;
            _chatBox.SelectionLength = 0;
            _chatBox.SelectionColor  = fg;
            _chatBox.SelectionBackColor = bg;
            _chatBox.SelectionFont   = isBold
                ? new Font(_chatBox.Font, FontStyle.Bold)
                : _chatBox.Font;
            _chatBox.AppendText(text);
            _chatBox.ScrollToCaret();
        }

        private void AppendLine(string text = "") => _chatBox.AppendText(text + "\n");

        // ─────────────────────────────────────────
        //  Status bar
        // ─────────────────────────────────────────
        private void UpdateStatus(string sentiment)
        {
            (string emoji, string hint, Color col) = sentiment switch
            {
                "worried"    => ("😟", "Feeling worried? Here are some reassuring tips.", _warnColour),
                "curious"    => ("🤓", "Great curiosity! Keep exploring cybersecurity.", _accentBlue),
                "frustrated" => ("😤", "Hang in there — cybersecurity doesn't have to be hard!", _warnColour),
                _            => ("💡", "Try asking: \"Tell me about phishing\" or \"Give me a password tip\"", _textMuted)
            };
            _statusLabel.Text      = $"{emoji} {hint}";
            _statusLabel.ForeColor = col;
        }

        // ─────────────────────────────────────────
        //  Utility
        // ─────────────────────────────────────────
        private Button CreateIconButton(string text, Point loc, int width, EventHandler handler)
        {
            var btn = new Button
            {
                Text      = text,
                Location  = loc,
                Size      = new Size(width, 30),
                BackColor = _bgInput,
                ForeColor = _textMuted,
                Font      = new Font("Segoe UI", 9f),
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = _textMuted;
            btn.FlatAppearance.BorderSize  = 1;
            btn.Click += handler;
            return btn;
        }

        private void PlayTick()
        {
            try { SystemSounds.Asterisk.Play(); }
            catch { /* ignore sound errors */ }
        }

        private static string GetAsciiArt() =>
@"  ____      _               ___
 / ___|   _| |__   ___ _ __/ __| ___  ___
| |  | | | | '_ \ / _ \ '__\__ \/ _ \/ __|
| |__| |_| | |_) |  __/ |  ___) |  __/ (__
 \____\__, |_.__/ \___|_| |____/ \___|\___|
      |___/  Awareness Chatbot  v2.0";
    }
}
