
/*
    PocketPotentiostat

    Copyright (C) 2019 Yasuo Matsubara

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA
*/

using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using Color = System.Drawing.Color;

namespace Voltammogrammer
{
    public static class ChartColorPallets
    {
        public static List<Color> Bright
            => new List<Color>() {
                "#008000".FromHex(),
                "#0000FF".FromHex(),
                "#800080".FromHex(),
                "#00FF00".FromHex(),
                "#FF00FF".FromHex(),
                "#008080".FromHex(),
                "#FFFF00".FromHex(),
                "#808080".FromHex(),
                "#00FFFF".FromHex(),
                "#000080".FromHex(),
                "#800000".FromHex(),
                "#FF0000".FromHex(),
                "#808000".FromHex(),
                "#C0C0C0".FromHex(),
                "#FF6347".FromHex(),
                "#FFE4B5".FromHex()
        };
        public static List<Color> GreyScale
            => new List<Color>() {
                "#C8C8C8".FromHex(),
                "#BDBDBD".FromHex(),
                "#B2B2B2".FromHex(),
                "#A7A7A7".FromHex(),
                "#9C9C9C".FromHex(),
                "#919191".FromHex(),
                "#868686".FromHex(),
                "#7B7B7B".FromHex(),
                "#707070".FromHex(),
                "#656565".FromHex(),
                "#5A5A5A".FromHex(),
                "#4F4F4F".FromHex(),
                "#444444".FromHex(),
                "#393939".FromHex(),
                "#2E2E2E".FromHex(),
                "#232323".FromHex()
        };
        public static List<Color> Excel
            => new List<Color>() {
                "#9999FF".FromHex(),
                "#993366".FromHex(),
                "#FFFFCC".FromHex(),
                "#CCFFFF".FromHex(),
                "#660066".FromHex(),
                "#FF8080".FromHex(),
                "#0066CC".FromHex(),
                "#CCCCFF".FromHex(),
                "#000080".FromHex(),
                "#FF00FF".FromHex(),
                "#FFFF00".FromHex(),
                "#00FFFF".FromHex(),
                "#800080".FromHex(),
                "#800000".FromHex(),
                "#008080".FromHex(),
                "#0000FF".FromHex()
        };
        public static List<Color> Light
            => new List<Color>() {
                "#E6E6FA".FromHex(),
                "#FFF0F5".FromHex(),
                "#FFDAB9".FromHex(),
                "#FFFACD".FromHex(),
                "#FFE4E1".FromHex(),
                "#F0FFF0".FromHex(),
                "#F0F8FF".FromHex(),
                "#F5F5F5".FromHex(),
                "#FAEBD7".FromHex(),
                "#E0FFFF".FromHex()
        };
        public static List<Color> Pastel
            => new List<Color>() {
                "#87CEEB".FromHex(),
                "#32CD32".FromHex(),
                "#BA55D3".FromHex(),
                "#F08080".FromHex(),
                "#4682B4".FromHex(),
                "#9ACD32".FromHex(),
                "#40E0D0".FromHex(),
                "#FF69B4".FromHex(),
                "#F0E68C".FromHex(),
                "#D2B48C".FromHex(),
                "#8FBC8B".FromHex(),
                "#6495ED".FromHex(),
                "#DDA0DD".FromHex(),
                "#5F9EA0".FromHex(),
                "#FFDAB9".FromHex(),
                "#FFA07A".FromHex()
        };
        public static List<Color> EarthTones
            => new List<Color>() {
                "#FF8000".FromHex(),
                "#B8860B".FromHex(),
                "#C04000".FromHex(),
                "#6B8E23".FromHex(),
                "#CD853F".FromHex(),
                "#C0C000".FromHex(),
                "#228B22".FromHex(),
                "#D2691E".FromHex(),
                "#808000".FromHex(),
                "#20B2AA".FromHex(),
                "#F4A460".FromHex(),
                "#00C000".FromHex(),
                "#8FBC8B".FromHex(),
                "#B22222".FromHex(),
                "#8B4513".FromHex(),
                "#C00000".FromHex()
        };
        public static List<Color> SemiTransparent
            => new List<Color>() {
                "#FF0000".FromHex(),
                "#00FF00".FromHex(),
                "#0000FF".FromHex(),
                "#FFFF00".FromHex(),
                "#00FFFF".FromHex(),
                "#FF00FF".FromHex(),
                "#AA7814".FromHex(),
                "#FF0000".FromHex(),
                "#00FF00".FromHex(),
                "#0000FF".FromHex(),
                "#FFFF00".FromHex(),
                "#00FFFF".FromHex(),
                "#FF00FF".FromHex(),
                "#AA7814".FromHex(),
                "#647832".FromHex(),
                "#285A96".FromHex()
        };
        public static List<Color> Berry
            => new List<Color>() {
                "#8A2BE2".FromHex(),
                "#BA55D3".FromHex(),
                "#4169E1".FromHex(),
                "#C71585".FromHex(),
                "#0000FF".FromHex(),
                "#8A2BE2".FromHex(),
                "#DA70D6".FromHex(),
                "#7B68EE".FromHex(),
                "#C000C0".FromHex(),
                "#0000CD".FromHex(),
                "#800080".FromHex()
        };
        public static List<Color> Chocolate
            => new List<Color>() {
                "#A0522D".FromHex(),
                "#D2691E".FromHex(),
                "#8B0000".FromHex(),
                "#CD853F".FromHex(),
                "#A52A2A".FromHex(),
                "#F4A460".FromHex(),
                "#8B4513".FromHex(),
                "#C04000".FromHex(),
                "#B22222".FromHex(),
                "#B65C3A".FromHex()
        };
        public static List<Color> Fire
            => new List<Color>() {
                "#FFD700".FromHex(),
                "#FF0000".FromHex(),
                "#FF1493".FromHex(),
                "#DC143C".FromHex(),
                "#FF8C00".FromHex(),
                "#FF00FF".FromHex(),
                "#FFFF00".FromHex(),
                "#FF4500".FromHex(),
                "#C71585".FromHex(),
                "#DDE221".FromHex()
        };
        public static List<Color> SeaGreen
            => new List<Color>() {
                "#2E8B57".FromHex(),
                "#66CDAA".FromHex(),
                "#4682B4".FromHex(),
                "#008B8B".FromHex(),
                "#5F9EA0".FromHex(),
                "#3CB371".FromHex(),
                "#48D1CC".FromHex(),
                "#B0C4DE".FromHex(),
                "#8FBC8B".FromHex(),
                "#87CEEB".FromHex()
        };
        public static ArrayList BrightPastel
            = new ArrayList() {
                "#418CF0".FromHex(),
                "#FCB441".FromHex(),
                "#E0400A".FromHex(),
                "#056492".FromHex(),
                "#BFBFBF".FromHex(),
                "#1A3B69".FromHex(),
                "#FFE382".FromHex(),
                "#129CDD".FromHex(),
                "#CA6B4B".FromHex(),
                "#005CDB".FromHex(),
                "#F3D288".FromHex(),
                "#506381".FromHex(),
                "#F1B9A8".FromHex(),
                "#E0830A".FromHex(),
                "#7893BE".FromHex()
        };

        public static ArrayList Custom
            => new ArrayList() {
                "Black".FromHex(),
                "Red".FromHex(),
                "Blue".FromHex(),
                "Green".FromHex(),
                "Orange".FromHex(),
                "MediumPurple".FromHex(),
                "DeepSkyBlue".FromHex(),
                "Magenta".FromHex(),
                "Tan".FromHex(),
                "Lime".FromHex()
        };

        private static Color FromHex(this string hex) => ColorTranslator.FromHtml(hex);
    }
}
