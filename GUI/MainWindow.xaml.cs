﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.IO;
using Microsoft.Win32;
using GraphComponent.GraphBuilder;
using GraphComponent.SettingWindow;
using GraphComponent.PopupWindow;
using GraphComponent.GraphConverter;
using GraphComponent;
using System.Windows.Controls;
using System;
using System.Threading;

namespace GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //GraphPane popupmenu event handlers
            GraphView.On_MenuItem_ChangeID += MenuItem_ChangeID_Click;
            GraphView.ShowMessage += ShowMessage;
        }

        #region Message Handling

        private void ShowMessage(string message, MessageBoxImage icon)
        {
            string title = "";
            switch (icon)
            {
                case MessageBoxImage.None:
                case MessageBoxImage.Question:
                    title = "Сообщение";
                    break;
                case MessageBoxImage.Error:
                    title = "Ошибка";
                    break;
                case MessageBoxImage.Warning:
                    title = "Предупреждение";
                    break;
                case MessageBoxImage.Information:
                    title = "Информация";
                    break;
                default:
                    break;
            }
            MessageBox.Show(this, message, title, MessageBoxButton.OK, icon);
        }

        bool InAquiredMode(ViewModel.TreeMode mode)
        {
            if (ViewModel.Mode != mode)
            {
                if (mode == ViewModel.TreeMode.DIRECTED)
                    ShowMessage("Доступно только для ориентированных деревьев", MessageBoxImage.Information);
                else
                    ShowMessage("Доступно только для неориентированных деревьев", MessageBoxImage.Information);
                return false;
            }
            return true;
        }

        bool HandleCheckOrientedTreeStatus()
        {
            if (!GraphBuilderStrategy.ValidateOrientedGraph(GraphView.ViewModel.GetWorkTree()))
            {
                ShowMessage("Граф НЕ является ориентированным деревом", MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        bool HandleCheckNonOrientedTreeStatus()
        {
            if (!GraphBuilderStrategy.ValidateNonOrientedGraph(GraphView.ViewModel.GetWorkTree()))
            {
                ShowMessage("Граф НЕ является деревом", MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        #endregion

        #region Menu Handling

        private void OpenFile_OnClick(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                ConstructByPrufer(File.ReadAllText(openFileDialog.FileName));
                InfoBar.Content = "";
            }
        }

        private void SaveFile_OnClick(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if (GraphView.ViewModel.GetWorkTree().IsVerticesEmpty)
            {
                ShowMessage("Отсутствует граф для сохранения", MessageBoxImage.Error);
                return;
            }

            SaveFileDialog saveDialogFile = new SaveFileDialog()
            {
                Filter = GraphConverter.SupportedFilters.Values.Aggregate((cur, next) => cur + "|" + next)
            };
            if (saveDialogFile.ShowDialog() == true)
            {
                string graphCode = GraphConverter.CodeGraphToString(GraphView.ViewModel.GetWorkTree(), 
                    GraphConverter.SupportedFilters.ElementAt(saveDialogFile.FilterIndex - 1).Key);
                try
                {
                    StreamWriter file = new StreamWriter(saveDialogFile.FileName);
                    file.WriteLine(graphCode);
                    file.Close();
                }
                catch
                {
                    ShowMessage("Невозможно сохранить граф в файл: \n" + saveDialogFile.FileName, MessageBoxImage.Error);
                }
            }
        }

        private void Close_OnClick(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void New_OnClick(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            GraphView.ViewModel.Tree = new Tree();
            InfoBar.Content = "";
            PruferTextBox.Clear();
        }

        private void OpenTasks_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem).IsChecked)
                TaskGrid.Visibility = Visibility.Visible;
            else
                TaskGrid.Visibility = Visibility.Collapsed;
        }

        private void OpenTheory_Click(object sender, RoutedEventArgs e)
        {
            Window theoryWindow = new TheoryWindow();
            theoryWindow.Show();
        }

        #endregion

        #region ToolBar Handling

        private void NewVertex_OnClick(object sender, RoutedEventArgs e)
        {
            GraphView.AddNewVertex();
            //PopupWindow popup = new PopupWindow("Создать вершину", "Введите номер новой вершины: ", "Создать")
            //{
            //    Owner = this
            //};
            //if (popup.ShowDialog() == true)
            //    GraphView.AddNewVertex(popup.Result);
        }

        private void NewEdge_OnClick(object sender, RoutedEventArgs e)
        {
            PopupWindowWith2Boxes popup = new PopupWindowWith2Boxes("Создать ребро", "Введите номера двух вершин, которые хотите соединить: ", "Соединить")
            {
                Owner = this
            };
            if (popup.ShowDialog() == true)
                GraphView.AddNewEdge(popup.Result1, popup.Result2);
        }

        #endregion

        #region Workflow Handling

        private void ConstructByPrufer_OnClick(object sender, RoutedEventArgs e)
        {
            List<int> pruferCode = null;

            try
            {
                pruferCode = PruferTextBox.Text.Trim(' ').Split(' ', ',', ';').Select(int.Parse).ToList();
            }
            catch (FormatException)
            {
                ShowMessage("Неверный формат", MessageBoxImage.Error);
                return;
            }

            if (!GraphBuilderStrategy.ValidateCode(pruferCode))
            {
                ShowMessage("Неверный код Прюфера", MessageBoxImage.Error);
                return;
            }
            GraphView.ViewModel.Tree = GraphBuilderStrategy.CodeToGraph(pruferCode);
            UpdateLayoutThroughViewModel();
            InfoBar.Content = "";
        }

        private void GetPrufer_OnClick(object sender, RoutedEventArgs e)
        {
            if (!HandleCheckNonOrientedTreeStatus())
                return;

            bool isEmpty = GraphView.ViewModel.GetWorkTree().IsVerticesEmpty;
            InfoBar.Content = isEmpty ? "" : "Код Прюфера: " + string.Join(" ", GraphBuilderStrategy.GraphToCode(GraphView.ViewModel.GetWorkTree()).ToArray());
        }

        private void Numerate_OnClick(object sender, RoutedEventArgs e)
        {
            if (!HandleCheckNonOrientedTreeStatus())
                return;

            var mapId = Numerator.GetIDMap(GraphView.ViewModel.GetWorkTree());
            foreach (var pair in mapId)
            {
                pair.Key.ID = pair.Value;
            }
        }

        #endregion

        #region Graph Control

        private int MenuItem_ChangeID_Click(object sender, RoutedEventArgs e)
        {
            PopupWindow popup = new PopupWindow("Изменить номер", "Введите новый номер вершины: ", "Сохранить")
            {
                Owner = this
            };

            if (popup.ShowDialog() == true)
                return popup.Result;
            else
                return -1;
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            UpdateLayoutThroughViewModel();
        }

        private void CheckTree_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Mode == ViewModel.TreeMode.DIRECTED)
            {
                if (!GraphBuilderStrategy.ValidateOrientedGraph(GraphView.ViewModel.GetWorkTree()))
                    ShowMessage("Граф НЕ является ориентированным деревом", MessageBoxImage.Information);
                else 
                    ShowMessage("Граф является ориентированным деревом", MessageBoxImage.Information);
            }
            else
            {
                if (!GraphBuilderStrategy.ValidateNonOrientedGraph(GraphView.ViewModel.GetWorkTree()))
                    ShowMessage("Граф НЕ является неориентированным деревом", MessageBoxImage.Information);
                else
                    ShowMessage("Граф является неориентированным деревом", MessageBoxImage.Information);
            }
        }

        private void FindRoot_Click(object sender, RoutedEventArgs e)
        {
            if (!InAquiredMode(ViewModel.TreeMode.DIRECTED))
                return;

            if (HandleCheckOrientedTreeStatus())
            {
                GraphView.ViewModel.SetRoot(GraphView.ViewModel.GetWorkTree().FindRoot());
            }
        }

        private void GetLength_Click(object sender, RoutedEventArgs e)
        {
            if (!HandleCheckNonOrientedTreeStatus())
                return;

            int length = GraphView.ViewModel.GetWorkTree().GetLength();
            int minLength = GraphView.ViewModel.GetWorkTree().GetLength(Numerator.GetIDMap(GraphView.ViewModel.GetWorkTree(), true));
            InfoBar.Content = "Текущая длина: " + length + "  Минимальная длина: " + minLength;
        }

        private void MinRoot_Click(object sender, RoutedEventArgs e)
        {
            if (!InAquiredMode(ViewModel.TreeMode.DIRECTED))
                return;

            Tree tree = new Tree();
            tree.AddVertexRange(GraphView.ViewModel.GetWorkTree().Vertices);
            tree.AddEdgeRange(GraphView.ViewModel.GetWorkTree().Edges);
            tree.Root = GraphView.ViewModel.GetWorkTree().Root;

            var minLength = tree.GetLength();
            var minRoot = tree.Root;
            if (minRoot == null)
            {
                minRoot = tree.FindRoot();
            }
            foreach ( var v in tree.Vertices)
            {
                tree.Root = v;
                tree.ReconstructTree();
                //GraphView.DoNumerateStep();
                if (tree.GetLength() < minLength)
                {
                    //Thread.Sleep(10000);
                    minLength = tree.GetLength();
                    minRoot = tree.Root;
                }
            }
            GraphView.ViewModel.SetRoot(minRoot);
            //GraphView.DoNumerateStep();
            InfoBar.Content = "Длина минимальной конфигурации: " + minLength;
        }

        void UpdateLayoutThroughViewModel()
        {
            (GraphView.DataContext as ViewModel).UpdateLayout();
        }

        void ConstructByPrufer(string str)
        {
            List<int> pruferCode;
            try
            {
                pruferCode = str.Split(' ', ',', ';').Select(int.Parse).ToList();
            }
            catch
            {
                ShowMessage("Неверный формат", MessageBoxImage.Error);
                return;
            }

            if (!GraphBuilderStrategy.ValidateCode(pruferCode))
            {
                ShowMessage("Неверный код Прюфера", MessageBoxImage.Error);
                return;
            }
            GraphView.ViewModel.Tree = GraphBuilderStrategy.CodeToGraph(pruferCode);
            UpdateLayoutThroughViewModel();
        }

        #endregion

        private void Numerate_Orient_Click(object sender, RoutedEventArgs e)
        {
            if (!InAquiredMode(ViewModel.TreeMode.DIRECTED))
                return;

            if (!HandleCheckOrientedTreeStatus())
            {
                return;
            }

            var root = GraphView.ViewModel.Tree.Root;
            if (root == null)
            {
                root = GraphView.ViewModel.Tree.FindRoot();
                GraphView.ViewModel.SetRoot(root);
            }
            if (root == null)
            {
                ShowMessage("Укажите корень", MessageBoxImage.Information);
                return;
            }
            GraphView.DoNumerateStep(root);
        }

        private void Switch_Click(object sender, RoutedEventArgs e)
        {
            GraphView.Switch();
        }
    }
}
