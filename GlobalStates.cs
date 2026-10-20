// Copyright (C) 2026 Dovintc
// This project is licensed under the GNU General Public License, version 3.0 only.
// The full license text is available at: https://gnu.org
namespace QueryOS_Freelancer.GlobalStates;

public static class GlobalStates {
    public static class Window {
        public static int width { get; set; }
        public static int height { get; set; }
    }
    public static class User {
        public static string name = "Mikle";
        public static bool isRoot = false; 
        public static int stage = 0;
    }
}
