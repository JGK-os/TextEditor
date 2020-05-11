using System;
using System.IO;

namespace OpenTXT
{
    internal class Program
    {
        #region Shared Variables

        private const  int  maxPageRows   = 1000;
        private const  int  maxPageChars  = 1000;
        private static int  usedLines     = 0;
        private static int  col           = 0;
        private static int  row           = 0;

        private static bool escPressed    = false;

        private static StreamWriter writer;
        private static char?[,]     page;
        public  static string       fileName;

        #endregion Shared Variables

        #region Page Viewer

        /// <summary>
        /// Create and set to null all cells of the page
        /// </summary>
        /// <param name="page">2D array of char? that rapresents the page</param>
        private static void PageCreate(out char?[,] page)
        {
            page = new char?[maxPageRows , maxPageChars];

            for (int r = 0 ; r < page.GetLength( 0 ) ; r++)
                for (int c = 0 ; c < page.GetLength( 1 ) ; c++)
                    page[r , c] = null;
        }

        /// <summary>
        /// Read the file content and copy it inside page
        /// </summary>
        /// <param name="readLine">File line</param>
        /// <param name="readingFlush">Reading flush</param>
        private static void ReadFile(string[] readLine , StreamReader readingFlush)
        {
            //Copy the file to a string array
            while ((readLine[usedLines++] = readingFlush.ReadLine()) != null) { }

            char[] charLine;
            int    column;

            //Copy the file content inside the page
            for (int r = 0 ; r < usedLines - 1 ; r++)
            {
                charLine = readLine[r].ToCharArray();
                column = 0;

                foreach (char item in charLine)
                    page[r , column++] = item;
            }
        }

        /// <summary>
        /// Print the page on console
        /// </summary>
        private static void PrintPage()
        {
            Console.Clear();

            //Print the command help
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.ForegroundColor = ConsoleColor.DarkCyan;

            Console.Write( "ESC" );

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;

            Console.Write( " = save + exit" );

            //Set the cursor immediately after the command help
            Console.SetCursorPosition( 0 , 1 );

            //Print on console
            for (int r = 0 ; r <= usedLines ; r++)
                Console.WriteLine( RowsCompressor( r , r == row ? " " : "" ) );
        }

        /// <summary>
        /// Save the result to the file
        /// </summary>
        private static void SaveFile()
        {
            if (escPressed)
            {
                //Create the file if it doesn't exists
                if (File.Exists( fileName ))
                    File.WriteAllText( fileName , string.Empty );

                using (writer = new StreamWriter( new FileStream( fileName , FileMode.OpenOrCreate , FileAccess.Write , FileShare.ReadWrite ) ))
                {
                    Console.SetOut( writer );

                    for (int r = 0 ; r < usedLines ; r++)
                        Console.WriteLine( RowsCompressor( r , "" ) );
                }
            }

            Console.Clear();
        }

        /// <summary>
        /// Create a string with the content of a page row
        /// </summary>
        /// <param name="pageRow">Row of the page</param>
        /// <param name="whenNull">String to print if the cell is null</param>
        /// <returns>String</returns>
        private static string RowsCompressor(int pageRow , string whenNull)
        {
            string compressedRow = "";

            for (int c = 0 ; c < page.GetLength( 1 ) ; c++)
            {
                compressedRow += page[pageRow , c] == null ? whenNull : Convert.ToString( page[pageRow , c] );

                if (page[pageRow , c] == null)
                    c = page.GetLength( 1 );
            }
            return compressedRow;
        }

        #endregion Page Viewer

        #region Writing

        #region Key Pressed

        /// <summary>
        /// When Enter is pressed
        /// </summary>
        /// <param name="row">Row of the page</param>
        private static void PressedEnter(int row)
        {
            for (int r = page.GetLength( 0 ) - 1 ; r >= row ; r--) //Scrolling
            {
                if (r != row) //I used this instead of (r != row ? page[r - 1 , c] : null) cause it has to do it only once per row
                {
                    for (int c = 0 ; c < page.GetLength( 1 ) ; c++)
                        page[r , c] = page[r - 1 , c];
                }
                else
                {
                    page[r , 0] = ' ';
                    for (int c = 1 ; c < page.GetLength( 1 ) ; c++)
                        page[r , c] = null;
                }
            }

            if (page[row - 1 , col] != null)
            {
                for (int i = 0 ; col + i < page.GetLength( 1 ) ; i++)
                {
                    page[row , i] = page[row - 1 , col + i];
                    page[row - 1 , col + i] = null;

                    if (page[row - 1 , col + i + 1] == null)
                        i = page.GetLength( 1 );
                }
            }

            col = 0;
            usedLines++;

            //Print
            for (int r = row - 1 ; r <= usedLines ; r++)
            {
                Console.SetCursorPosition( 0 , r + 1 );
                Console.Write( new string( ' ' , Console.WindowWidth ) );
                Console.SetCursorPosition( 0 , r + 1 );
                Console.WriteLine( RowsCompressor( r , " " ) );
            }
        }

        /// <summary>
        /// When Text pressed
        /// </summary>
        /// <param name="keyPressed">key pressed</param>
        private static void Text(ConsoleKeyInfo keyPressed)
        {
            if (page[row , col] != null)
            {
                for (int i = page.GetLength( 1 ) - 1 ; i > col ; i--)
                {
                    page[row , i] = page[row , i - 1];
                }
            }

            page[row , col++] = keyPressed.KeyChar;

            Console.SetCursorPosition( 0 , row + 1 );

            Console.Write( RowsCompressor( row , " " ) );
        }

        /// <summary>
        /// When Backspace is pressed
        /// </summary>
        private static void PressedBackspace()
        {
            if (col != 0) //Delete char
            {
                if (page[row , col] != null)
                {
                    for (int i = --col ; i < page.GetLength( 1 ) - 2 ; i++)
                        page[row , i] = page[row , i + 1];
                }
                else
                {
                    page[row , --col] = null;
                }

                //Print
                Console.SetCursorPosition( 0 , row + 1 );

                Console.Write( RowsCompressor( row , " " ) );
            }
            else if (row != 0) //Scrolling if the are no more char in the line
            {
                int colTmp1 = col = FindFirstNull(row - 1);
                int colTmp2 = FindFirstNull(row);

                for (int i = 0 ; i <= colTmp2 ; i++)
                    page[row - 1 , colTmp1++] = page[row , i];

                for (int r = row ; r <= usedLines ; r++)
                {
                    colTmp1 = FindFirstNull( r );
                    colTmp2 = FindFirstNull( r + 1 );

                    for (int c = 0 ; c < (colTmp1 > colTmp2 ? colTmp1 : colTmp2) ; c++)
                        page[r , c] = page[r + 1 , c];
                }

                //Print
                for (int r = --row ; r <= usedLines ; r++)
                {
                    Console.SetCursorPosition( 0 , r + 1 );
                    Console.Write( new string( ' ' , Console.WindowWidth ) );
                    Console.SetCursorPosition( 0 , r + 1 );
                    Console.WriteLine( RowsCompressor( r , " " ) );
                }

                usedLines--;
            }
        }

        /// <summary>
        /// When Delete is pressed
        /// </summary>
        private static void PressedDelete()
        {
            if (page[row , col] != null) //Delete char
            {
                for (int i = col ; i < page.GetLength( 1 ) - 2 ; i++)
                {
                    page[row , i] = page[row , i + 1];

                    if (page[row , i + 1] == null)
                        i = page.GetLength( 1 );
                }

                page[row , page.GetLength( 1 ) - 1] = null;

                //Print
                Console.SetCursorPosition( 0 , row + 1 );

                Console.Write( RowsCompressor( row , " " ) );
            }
            else if (col == FindFirstNull( row ) && row < usedLines) //Scrolling if the cursor is in the last written cell of the line
            {
                int colTmp1 = FindFirstNull( row );
                int colTmp2 = FindFirstNull( row + 1 );

                for (int i = 0 ; i <= colTmp2 ; i++)
                    page[row , colTmp1++] = page[row + 1 , i];

                for (int r = row + 1 ; r <= usedLines ; r++)
                {
                    colTmp1 = FindFirstNull( r );
                    colTmp2 = FindFirstNull( r + 1 );

                    for (int c = 0 ; c < (colTmp1 > colTmp2 ? colTmp1 : colTmp2) ; c++)
                        page[r , c] = page[r + 1 , c];
                }

                //Print
                for (int r = row ; r <= usedLines ; r++)
                {
                    Console.SetCursorPosition( 0 , r + 1 );
                    Console.Write( new string( ' ' , Console.WindowWidth ) );
                    Console.SetCursorPosition( 0 , r + 1 );
                    Console.WriteLine( RowsCompressor( r , " " ) );
                }

                usedLines--;
            }
        }

        /// <summary>
        /// Up or Down Arrow pressed
        /// </summary>
        /// <param name="key">Key Pressed</param>
        private static void ArrowsUpDown(ConsoleKey key)
        {
            //Set offset -1 if Up, 1 if Down
            int offset = ( key == ConsoleKey.UpArrow ? -1 : 1 );

            if (!(((row == page.GetLength( 0 ) - 1 || row == usedLines - 1) && offset == 1) || (row == 0 && offset == -1)))
            {
                row += offset;

                if (page[row , col] == null)
                    col = FindFirstNull( row );
            }
        }

        /// <summary>
        /// Left or Right Arrow pressed
        /// </summary>
        /// <param name="key">Key Pressed</param>
        private static void ArrowsLeftRight(ConsoleKey key)
        {
            //Set offset -1 if Left, 1 if Right
            int offset = ( key == ConsoleKey.LeftArrow ? -1 : 1 );

            if (!((col == page.GetLength( 1 ) - 1 && offset == 1) || (col == 0 && offset == -1)) && (col <= FindFirstNull( row )) && !(col == FindFirstNull( row ) && offset == 1))
                col += offset;
        }

        #endregion Key Pressed

        /// <summary>
        /// Find the first null cell in a row
        /// </summary>
        /// <param name="pageRow">Page row</param>
        /// <returns>Column of the first null cell</returns>
        private static int FindFirstNull(int pageRow)
        {
            for (int i = 0 ; i < page.GetLength( 1 ) ; i++)
            {
                if (page[pageRow , i] == null)
                    return i;
            }
            return page.GetLength( 1 );
        }

        /// <summary>
        /// Actions when input is received
        /// </summary>
        private static void OnDataReceive()
        {
            ConsoleKeyInfo keyPressed = Console.ReadKey(true);

            if (escPressed = keyPressed.Key == ConsoleKey.Escape) //When Esc is pressed
            { }
            else if (keyPressed.Key == ConsoleKey.Enter && row < page.GetLength( 0 ) - 1) //When Enter is pressed
            {
                PressedEnter( ++row );
            }
            else if ((char.IsLetterOrDigit( keyPressed.KeyChar ) || char.IsPunctuation( keyPressed.KeyChar ) || char.IsWhiteSpace( keyPressed.KeyChar ) || char.IsSymbol( keyPressed.KeyChar )) && col < page.GetLength( 1 ))
            {   //When Text is pressed
                Text( keyPressed );
            }
            else if (keyPressed.Key == ConsoleKey.Backspace) //When Backspace is pressed
            {
                PressedBackspace();
            }
            else if (keyPressed.Key == ConsoleKey.Delete) //When Delete is pressed
            {
                PressedDelete();
            }
            else if (keyPressed.Key == ConsoleKey.UpArrow || keyPressed.Key == ConsoleKey.DownArrow) // When Up/Down Arrow is pressed
            {
                ArrowsUpDown( keyPressed.Key );
            }
            else if (keyPressed.Key == ConsoleKey.LeftArrow || keyPressed.Key == ConsoleKey.RightArrow) // When Left/Right Arrow is pressed
            {
                ArrowsLeftRight( keyPressed.Key );
            }
        }

        #endregion Writing

        private static void Ciao(string[] args)
        {
            fileName = args[0];

            PageCreate( out page ); //Set the page cells to null

            if (File.Exists( fileName ))
            {
                using (FileStream fileReader = new FileStream( fileName , FileMode.Open , FileAccess.Read , FileShare.ReadWrite ))
                {
                    using (StreamReader reader = new StreamReader( fileReader )) // Open the file
                    {
                        string[] line     = new string[maxPageRows];
                        line[usedLines++] = reader.ReadLine();

                        if (line[0] != null)
                            ReadFile( line , reader );
                    }
                }
            }

            PrintPage();

            while (!escPressed) //Exit if ESC pressed
            {
                Console.SetCursorPosition( col , row + 1 ); //For the buffer

                OnDataReceive();

                //PrintPage();
            }

            SaveFile(); //Print the page on a txt file
        }
    }
}