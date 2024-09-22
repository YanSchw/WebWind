using WebWind.Core;

namespace WebWind.Components;

public class Div : Component
{

    public Div()
    {
        JS.CreateDocumentElement("div", Id);
    }

}