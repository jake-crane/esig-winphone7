﻿#pragma checksum "C:\StepheVaio\Tests\CaveAVins.WP7.SL\WP7.ScanBarCode\ScanBarCode.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "6536F08F885E2459D432B22298F71E86"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.16830
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace CaveAVins.WP7.SL.View {
    
    
    public partial class ScanBarCode : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Shapes.Rectangle video;
        
        internal System.Windows.Shapes.Rectangle rectangleLeft;
        
        internal System.Windows.Shapes.Rectangle rectangleRight;
        
        internal System.Windows.Shapes.Rectangle rectangleTop;
        
        internal System.Windows.Shapes.Rectangle rectangleBottom;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/WP7.ScanBarCode;component/ScanBarCode.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.video = ((System.Windows.Shapes.Rectangle)(this.FindName("video")));
            this.rectangleLeft = ((System.Windows.Shapes.Rectangle)(this.FindName("rectangleLeft")));
            this.rectangleRight = ((System.Windows.Shapes.Rectangle)(this.FindName("rectangleRight")));
            this.rectangleTop = ((System.Windows.Shapes.Rectangle)(this.FindName("rectangleTop")));
            this.rectangleBottom = ((System.Windows.Shapes.Rectangle)(this.FindName("rectangleBottom")));
        }
    }
}
