//Copyright (C) 2018 - Luca Montanari <thunderluca93@gmail.com>
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace WinSteroid.App.Controls
{
    public class NavigationPageHeader : Control
    {
        public NavigationPageHeader()
        {
            this.DefaultStyleKey = typeof(NavigationPageHeader);
        }

        public string TitleText
        {
            get { return (string)GetValue(TitleTextProperty); }
            set { SetValue(TitleTextProperty, value); }
        }
        
        public static readonly DependencyProperty TitleTextProperty =
            DependencyProperty.Register(nameof(TitleText), typeof(string), typeof(NavigationPageHeader), new PropertyMetadata(string.Empty));

        public string ButtonGlyph
        {
            get { return (string)GetValue(ButtonGlyphProperty); }
            set { SetValue(ButtonGlyphProperty, value); }
        }

        public static readonly DependencyProperty ButtonGlyphProperty =
            DependencyProperty.Register(nameof(ButtonGlyph), typeof(string), typeof(NavigationPageHeader), new PropertyMetadata(null));


        public ICommand ButtonCommand
        {
            get { return (ICommand)GetValue(ButtonCommandProperty); }
            set { SetValue(ButtonCommandProperty, value); }
        }

        public static readonly DependencyProperty ButtonCommandProperty =
            DependencyProperty.Register(nameof(ButtonCommand), typeof(ICommand), typeof(NavigationPageHeader), new PropertyMetadata(null));

    }
}
