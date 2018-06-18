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

using System;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace WinSteroid.App.Controls
{
    public class AcrylicGrid : Grid
    {
        private readonly Compositor _compositor;
        private readonly SpriteVisual _spriteVisual;

        public AcrylicGrid()
        {
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            _spriteVisual = _compositor.CreateSpriteVisual();
            _spriteVisual.Size = new Vector2((float)this.ActualWidth, (float)this.ActualHeight);

            _spriteVisual.Brush = _compositor.CreateHostBackdropBrush();

            ElementCompositionPreview.SetElementChildVisual(this, _spriteVisual);

            var scalarKeyFrameAnimation = _compositor.CreateScalarKeyFrameAnimation();
            scalarKeyFrameAnimation.InsertKeyFrame(0.0f, 0.0f);
            scalarKeyFrameAnimation.InsertKeyFrame(1.0f, 1.0f);
            scalarKeyFrameAnimation.DelayTime = TimeSpan.Zero;
            scalarKeyFrameAnimation.Duration = TimeSpan.FromSeconds(1);
            scalarKeyFrameAnimation.Target = nameof(this.Opacity);

            _spriteVisual.StartAnimation(nameof(this.Opacity), scalarKeyFrameAnimation);

            this.SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_spriteVisual == null) return;

            _spriteVisual.Size = e.NewSize.ToVector2();
        }
    }
}
