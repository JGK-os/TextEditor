using System;
using System.IO;
using OS_Code.Core;

namespace External.Program.TextEditor
{
    internal class TextEditor
    {
        #region Variables

        private static string FileName { get; set; } = string.Empty;
        private static string[] Text { get; set; } = new string[10];

        private static int  Col           = 0;
        private static int  Row           = 0;

        #endregion Variables

        public static void Start(string name)
        {
            FileManagement( name );

            Write();

            Console.Clear();

            if (Text != null)
            {
                Save();
                Console.WriteLine( "Content has been saved to " + FileName );
            }

            Console.WriteLine( "Press any key to continue..." );
            Console.ReadKey( true );
        }

        #region Writing

        private static void Write()
        {
            while (true)
            {
                Console.SetCursorPosition( Col , Row );

                OnDataReceive();
            }
        }

        private static void OnDataReceive()
        {
            ConsoleKeyInfo keyPressed = Console.ReadKey(true);

            if (keyPressed.Key == ConsoleKey.Enter) //When Enter is pressed
            {
                if (Row < Text.Length - 1)
                {
                    Col = 0;
                    Row++;
                    Console.SetCursorPosition( 0 , Row );
                    Text[Row] = "";
                }
            }
            else if ((char.IsLetterOrDigit( keyPressed.KeyChar ) || char.IsPunctuation( keyPressed.KeyChar ) || char.IsWhiteSpace( keyPressed.KeyChar ) || char.IsSymbol( keyPressed.KeyChar )))
            {   //When Text is pressed
                if (Text[Row] == null)
                {
                    Text[Row] = "";
                }

                Text[Row] = Text[Row].Insert( Col , keyPressed.KeyChar.ToString() );
                Console.SetCursorPosition( 0 , Row );
                Console.WriteLine( Text[Row] );
                Col++;
            }
            else if (keyPressed.Key == ConsoleKey.Backspace) //When Backspace is pressed
            {
                Text[Row] = Text[Row].Remove( --Col , 1 );
                Console.SetCursorPosition( 0 , Row );
                for (int i = Col ; i < Text[Row].Length + 1 ; i++)
                    Console.Write( " " );
                Console.SetCursorPosition( 0 , Row );
                Console.Write( Text[Row] );
            }
            else if (keyPressed.Key == ConsoleKey.Delete) //When Delete is pressed
            {
            }
            else if (keyPressed.Key == ConsoleKey.UpArrow && Row > 0) // When Up/Down Arrow is pressed
            {
                Row--;
                if (Col > Text[Row].Length)
                {
                    Col = Text[Row].Length;
                }
            }
            else if (keyPressed.Key == ConsoleKey.DownArrow && Row < Text.Length - 1 && Text[Row + 1] != null)
            {
                Row++;
                if (Col > Text[Row].Length)
                {
                    Col = Text[Row].Length;
                }
            }
            else if (keyPressed.Key == ConsoleKey.LeftArrow) // When Left/Right Arrow is pressed
            {
                if (Col > 0)
                {
                    Col--;
                }
            }
            else if (keyPressed.Key == ConsoleKey.RightArrow)
            {
                if (Col < Text[Row].Length)
                {
                    Col++;
                }
            }
        }

        #endregion Writing

        #region File

        private static void Save()
        {
        }

        private static void FileManagement(string name)
        {
            if (name.Equals( string.Empty ))
            {
                Console.WriteLine( "Enter file's filename to open:" );
                Console.WriteLine( "If the specified file does not exist, it will be created." );
                name = Console.ReadLine().ToLower();
            }

            FileName = Kernel.pwd + name;

            try
            {
                if (File.Exists( FileName ))
                {
                    Console.WriteLine( "Found file!" );
                }
                else if (!File.Exists( FileName ))
                {
                    Console.WriteLine( "Creating file!" );
                    File.Create( FileName );
                }

                Console.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine( ex.Message );
            }
        }

        #endregion File
    }
}