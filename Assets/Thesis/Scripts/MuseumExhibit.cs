using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;


public class MuseumExhibit
{
    // Properties
    [BsonId]
    public ObjectId _id { get; set; }
    public int ExhibitID { get; set; }
    public String ExhibitName { get; set; }
    public String ConditionStatus { get; set; }
    public int PopularityScore { get; set; }
    public int YearCreated { get; set; }
    public int YearAcquired { get; set; }
    public int DisplayArea { get; set; } // in square meters
    public String Location { get; set; }
    public String Category { get; set; }

    // Constructor
    public MuseumExhibit(int exhibitID, string exhibitName, string conditionStatus, int popularityScore,
                    int yearCreated, int yearAcquired, int displayArea, string location, string category)
    {
        ExhibitID = exhibitID;
        ExhibitName = exhibitName;
        ConditionStatus = conditionStatus;
        PopularityScore = popularityScore;
        YearCreated = yearCreated;
        YearAcquired = yearAcquired;
        DisplayArea = displayArea;
        Location = location;
        Category = category;
    }

    // Method to Display Exhibit Details
    public void DisplayDetails()
    {
        Console.WriteLine($"Exhibit ID: {ExhibitID}");
        Console.WriteLine($"Exhibit Name: {ExhibitName}");
        Console.WriteLine($"Condition Status: {ConditionStatus}");
        Console.WriteLine($"Popularity Score: {PopularityScore}");
        Console.WriteLine($"Year Created: {YearCreated}");
        Console.WriteLine($"Year Acquired: {YearAcquired}");
        Console.WriteLine($"Display Area: {DisplayArea} m²");
        Console.WriteLine($"Location: {Location}");
        Console.WriteLine($"Category: {Category}");
    }
}
