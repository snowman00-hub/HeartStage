using System;
using System.Collections.Generic;

[Serializable]
public class MailData
{
    public string mailId;
    public string senderId;
    public string senderName;
    public string receiverId;
    public string title;
    public string content;
    public long timestamp;
    public bool isRead;
    public bool isRewarded;

    public List<ItemAttachment> itemList;

    public MailData(string mailId, string senderId, string senderName, string receiverId, string title, string content, List<ItemAttachment> itemList = null)
    {
        this.mailId = mailId;
        this.senderId = senderId;
        this.senderName = senderName;
        this.receiverId = receiverId;
        this.title = title;
        this.content = content;
        this.timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        this.isRead = false;
        this.isRewarded = false;
        this.itemList = itemList ?? new List<ItemAttachment>();
    }      

}

[Serializable]
public class ItemAttachment
{
    public string itemId;
    public int count;
    public ItemAttachment(string itemId, int count)
    {
        this.itemId = itemId;
        this.count = count;
    }
}
