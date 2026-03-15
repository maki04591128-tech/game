using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Gui;

/// <summary>
/// 会話ウィンドウ
/// </summary>
public partial class DialogueWindow : Window
{
    private readonly GameController _controller;
    private readonly string _npcId;
    private DialogueNode? _currentNode;
    private int _selectedChoice = -1;

    public DialogueWindow(GameController controller, DialogueNode node, string npcId)
    {
        InitializeComponent();
        _controller = controller;
        _npcId = npcId;

        ShowNode(node);
    }

    public void ShowNode(DialogueNode node)
    {
        _currentNode = node;
        SpeakerName.Text = node.SpeakerName;
        DialogueText.Text = node.Text;

        if (node.HasChoices && node.Choices != null)
        {
            var choices = new List<ChoiceViewModel>();
            for (int i = 0; i < node.Choices.Length; i++)
            {
                choices.Add(new ChoiceViewModel(i + 1, node.Choices[i].Text, i));
            }
            ChoiceList.ItemsSource = choices;
            ChoiceList.Visibility = Visibility.Visible;
            ContinueButton.Content = "選択 [Enter]";
        }
        else
        {
            ChoiceList.Visibility = Visibility.Collapsed;
            ChoiceList.ItemsSource = null;
            ContinueButton.Content = node.NextNodeId != null ? "続ける [Enter/Space]" : "閉じる [Enter/Space]";
        }
    }

    private void ChoiceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ChoiceList.SelectedItem is ChoiceViewModel selected)
        {
            _selectedChoice = selected.Index;
        }
    }

    private void ContinueButton_Click(object sender, RoutedEventArgs e)
    {
        AdvanceDialogue();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        _controller.EndDialogue();
        DialogResult = false;
        Close();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape:
                _controller.EndDialogue();
                DialogResult = false;
                Close();
                break;
            case Key.Enter:
            case Key.Space:
                AdvanceDialogue();
                break;
            case Key.D1 or Key.NumPad1:
                SelectChoice(0);
                break;
            case Key.D2 or Key.NumPad2:
                SelectChoice(1);
                break;
            case Key.D3 or Key.NumPad3:
                SelectChoice(2);
                break;
            case Key.D4 or Key.NumPad4:
                SelectChoice(3);
                break;
        }
        e.Handled = true;
    }

    private void SelectChoice(int index)
    {
        if (_currentNode?.HasChoices != true || _currentNode.Choices == null) return;
        if (index >= _currentNode.Choices.Length) return;

        _selectedChoice = index;
        ChoiceList.SelectedIndex = index;
        AdvanceDialogue();
    }

    private void AdvanceDialogue()
    {
        if (_currentNode == null) return;

        if (_currentNode.HasChoices && _currentNode.Choices != null)
        {
            if (_selectedChoice < 0 || _selectedChoice >= _currentNode.Choices.Length)
            {
                return;
            }

            bool result = _controller.TrySelectDialogueChoice(_selectedChoice, _npcId);
            if (!result)
            {
                _controller.EndDialogue();
                DialogResult = false;
                Close();
            }
            // 次のノードはGameControllerのOnShowDialogueイベント経由で表示される
            // ここでは一旦閉じて、MainWindowが再度開く
            DialogResult = true;
            Close();
        }
        else if (_currentNode.NextNodeId != null)
        {
            bool advanced = _controller.TryAdvanceDialogue();
            if (!advanced)
            {
                _controller.EndDialogue();
                DialogResult = false;
                Close();
            }
            else
            {
                DialogResult = true;
                Close();
            }
        }
        else
        {
            _controller.EndDialogue();
            DialogResult = false;
            Close();
        }
    }

    public class ChoiceViewModel
    {
        public string Number { get; }
        public string Text { get; }
        public int Index { get; }

        public ChoiceViewModel(int number, string text, int index)
        {
            Number = $"{number}.";
            Text = text;
            Index = index;
        }
    }
}
