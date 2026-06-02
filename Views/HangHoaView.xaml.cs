using System;
using System.Windows.Controls;

namespace QLHangTonKho.Views
{
    public partial class HangHoaView : UserControl
    {
        public bool IsEmbedded { get; set; } = false;

        public HangHoaView()
        {
            InitializeComponent();
        }
    }
}