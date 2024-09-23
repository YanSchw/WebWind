using WebWind.Core;

namespace WebWind.Components;

public class HStack : Component
{

    public HStack()
    {
        JS.CreateDocumentElement("h-stack", Id);
    }

}