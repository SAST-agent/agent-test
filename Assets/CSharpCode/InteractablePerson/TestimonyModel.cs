[System.Serializable]
public class TestimonyItem
{
    public string id;
    public string name;
    public string type;
    public string content;
}

[System.Serializable]
public class TestimonyResponse
{
    public int page;
    public int page_size;
    public int total;
    public int total_pages;
    public TestimonyItem[] items;
}
