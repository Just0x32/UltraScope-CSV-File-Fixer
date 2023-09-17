using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static UltraScopeCSVFileFixer.Model;

namespace UltraScopeCSVFileFixer
{
    internal class Model : INotifyPropertyChanged
    {
        private string[] filePaths = new string[0];
        private string tempFileExtension = ".tmp";
        private int lineCounter = 0;
        private int pointsQuantity = 24000000;
        private int pointsMinQuantity = 10;
        private string[] voltValue = new string[2] { "VOLT", "Volt" };
        private int voltLineIndex = 2;
        private char divider = ',';
        private string zero = "0.0";
        private bool isZeroOffset = false;
        private bool isEmptyPoints = false;

        public enum StatusTextIndex
        {
            NoText,
            Error,
            InvalidPointsQuantity,
            OutOfMemoryError,
            Fixing1,
            Fixing2,
            Fixing3,
            Fixed
        }

        private string[] statusTexts =
        {
            "",
            "Error",
            "Invalid points quantity!",
            "Out of memory!",
            "Fixing.",
            "Fixing..",
            "Fixing...",
            "Fixed!"
        };

        private string statusText = String.Empty;
        public string StatusText
        {
            get => statusText;
            private set
            {
                statusText = value;
                OnPropertyChanged();
            }
        }

        public void SetFilePath(string[] paths)
        {
            if (paths.Length != 0)
            {
                filePaths = paths;
                ChangeStatusText(StatusTextIndex.NoText);
            }
        }

        public void FixFile(string pointsQuantity, bool isZeroOffset, bool isEmptyPoints)
        {
            ChangeStatusText(StatusTextIndex.Fixing1);

            if (int.TryParse(pointsQuantity, out this.pointsQuantity) && this.pointsQuantity >= pointsMinQuantity)
            {
                this.isZeroOffset = isZeroOffset;
                this.isEmptyPoints = isEmptyPoints;

                Thread thread = new Thread(new ThreadStart(StartProcessing));
                thread.Start();

                void StartProcessing()
                {
                    StreamReader reader = StreamReader.Null;
                    StreamWriter writer = StreamWriter.Null;

                    for (int i = 0; i < filePaths.Length; i++)
                    {
                        ChangeFixingStatusText(i % 3);

                        try
                        {
                            reader = new StreamReader(filePaths[i]);
                            writer = new StreamWriter(filePaths[i] + tempFileExtension);
                            string? line;
                            lineCounter = 0;

                            while ((line = reader.ReadLine()) != null)
                            {
                                lineCounter++;

                                if (lineCounter == voltLineIndex)
                                {
                                    int startIndex = line.IndexOf(voltValue[0]);
                                    int endIndex = startIndex + voltValue[0].Length;

                                    if (startIndex >= 0)
                                        line = line[..startIndex] + voltValue[1] + line[endIndex..];

                                    if (isZeroOffset)
                                    {
                                        startIndex = line.IndexOf(voltValue[0]);

                                        if (startIndex >= 0)
                                            startIndex += voltValue[0].Length;
                                        else
                                        {
                                            startIndex = line.IndexOf(voltValue[1]);

                                            if (startIndex >= 0)
                                                startIndex += voltValue[1].Length;
                                        }

                                        if (startIndex >= 0)
                                        {
                                            startIndex++;
                                            endIndex = line.IndexOf(divider, startIndex);
                                            line = line[..startIndex] + zero + line[endIndex..];
                                        }

                                    }
                                }

                                if (line[^1] != divider)
                                    writer.WriteLine(line + divider);
                                else
                                    writer.WriteLine(line);
                            }

                            while (isEmptyPoints && lineCounter < this.pointsQuantity + 2)
                            {
                                writer.WriteLine((lineCounter - 2).ToString() + divider + zero + divider);
                                lineCounter++;
                            }

                            reader?.Dispose();
                            writer?.Dispose();
                            File.Delete(filePaths[i]);
                            File.Move(filePaths[i] + tempFileExtension, filePaths[i]);
                            ChangeStatusText(StatusTextIndex.Fixed);
                        }
                        catch (OutOfMemoryException)
                        {
                            ChangeStatusText(StatusTextIndex.OutOfMemoryError);
                        }
                        catch (Exception)
                        {
                            ChangeStatusText(StatusTextIndex.Error);
                        }
                        finally
                        {
                            reader?.Dispose();
                            writer?.Dispose();
                        }
                    }

                    void ChangeStatusText(StatusTextIndex statusTextIndex)
                    {
                        Application.Current.Dispatcher.Invoke(new Action(delegate
                        {
                            this.ChangeStatusText(statusTextIndex);
                        }));
                    }

                    void ChangeFixingStatusText(int index)
                    {
                        Application.Current.Dispatcher.Invoke(new Action(delegate
                        {
                            this.ChangeStatusText(index + (int)StatusTextIndex.Fixing1);
                        }));
                    }
                }
            }
            else
            {
                ChangeStatusText(StatusTextIndex.InvalidPointsQuantity);
            }
        }

        private void ChangeStatusText(StatusTextIndex statusTextIndex)
        {
            StatusText = statusTexts[(int)statusTextIndex];
        }

        private void ChangeStatusText(int index)
        {
            StatusText = statusTexts[index];
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
