using System;

[Serializable]
public class ShipUpgradeState
{
    public string shipId;

    // Sağ-sol manevra upgrade seviyesi
    public int horizontalManeuverLevel;

    // Yukarı-aşağı manevra upgrade seviyesi
    public int verticalManeuverLevel;

    // Mermi kapasitesi upgrade seviyesi
    public int ammoLevel;
}
