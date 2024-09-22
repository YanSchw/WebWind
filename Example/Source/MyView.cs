using WebWind.Components;
using WebWind.Core;

namespace Example.Source;

[Route("/")]
public class MyView : VStack
{
    private Int32 m_ClickCounter = 0;
    
    public MyView()
    {
        ClientInterface.Current.SetTitle("MyView");
        
        Paragraph p = CreateChild<Paragraph>("Hello World!");
        CreateChild<Button>("Click me.").AddOnClickedEvent(() =>
        {
            p.Text = $"The Button was clicked {++m_ClickCounter} times.";
        });
    }
}