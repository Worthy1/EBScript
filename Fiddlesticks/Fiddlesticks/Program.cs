using System;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;

namespace Fiddlesticks
{
    public static class Program
    {
        private static void Main(string[] args)
        {

            Loading.OnLoadingComplete += OnLoadingComplete;

        }

        private static void OnLoadingComplete(EventArgs args)
        {

            if (Player.Instance.ChampionName != "Fiddlesticks") return;

            Fiddle.Load();
        }
    }
}