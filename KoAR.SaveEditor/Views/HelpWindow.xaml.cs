#nullable enable

using System.Windows.Input;

namespace KoAR.SaveEditor.Views
{
    partial class HelpWindow
    {
        static HelpWindow()
        {
            CommandManager.RegisterClassCommandBinding(typeof(HelpWindow), new CommandBinding(ApplicationCommands.Close, HelpWindow.CloseCommand_Executed));
        }

        private static void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            HelpWindow window = (HelpWindow)sender;
            window.Close();
            e.Handled = true;
        }

        public HelpWindow()
        {
            this.InitializeComponent();
            this._textBox.Text = @"1. It is highly suggested to use this with as few items in your inventory as possible OR to damage your item's durability to an identifiable degree for better item identification if the item is not crafted.

2.Your saves are usually not in the same folder as the game.For Windows 7, they can be in C:\Program Files(x86)\Steam\userdata\< user_id >\102500\remote\.

3.When modifying item names, do NOT use special characters.

4.Items that are not crafted will show as ""unknown"".Edit these items at your own risk.

5.This is best used with crafted items that have a lot of properties, i.e.lots of flawless components.

6.You can only add as many as you can delete. For example, if the editor says you have 4 attributes(despite the item having more in -game), you can only delete the 4 and add up to 4.

7.Editing unique items or adding properties beyond the maximum detected will cause your file to not load.

8.The provided property.xml file is exhaustive but not complete. You can play around with in-game properties and make educated guesses on which attributes the unknown properties are by way of deleting and tracking.

9.While tricky, using the Hex Code edit can hypothetically give you any item with any property.It's unknown if this can bypass the unique item rule or the save corruption in 7. I haven't tested it personally.";

        }
    }
}
