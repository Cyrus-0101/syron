﻿//  .----------------.  .----------------.  .----------------.  .----------------.  .-----------------.
// | .--------------. || .--------------. || .--------------. || .--------------. || .--------------. |
// | |    _______   | || |  ____  ____  | || |  _______     | || |     ____     | || | ____  _____  | |
// | |   /  ___  |  | || | |_  _||_  _| | || | |_   __ \    | || |   .'    `.   | || ||_   \|_   _| | |
// | |  |  (__ \_|  | || |   \ \  / /   | || |   | |__) |   | || |  /  .--.  \  | || |  |   \ | |   | |
// | |   '.___`-.   | || |    \ \/ /    | || |   |  __ /    | || |  | |    | |  | || |  | |\ \| |   | |
// | |  |`\____) |  | || |    _|  |_    | || |  _| |  \ \_  | || |  \  `--'  /  | || | _| |_\   |_  | |
// | |  |_______.'  | || |   |______|   | || | |____| |___| | || |   `.____.'   | || ||_____|\____| | |
// | |              | || |              | || |              | || |              | || |              | |
// | '--------------' || '--------------' || '--------------' || '--------------' || '--------------' |
//  '----------------'  '----------------'  '----------------'  '----------------'  '----------------' 

namespace Syron

{
    internal static class Program
    {
        private static void Main()
        {
            var repl = new SyronRepl();
            repl.Run();
        }
    }

}
