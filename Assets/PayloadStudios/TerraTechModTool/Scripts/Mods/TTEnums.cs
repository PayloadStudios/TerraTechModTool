public enum BlockRarity
{
	Common,
	Uncommon,
	Rare
}

public enum BlockModule
{
	Spinner,
}

public enum BlockCategories
{
	Null,
	Control,
	Standard,
	Wheels,
	Weapons,
	Accessories,
	Power,
	Manufacturing, // Called Base in the game, should translate correctly
	Flight
}

public enum TextureSlot
{
	Main,
	Tracks,
	Extra,

	NUM_TEXTURE_SLOTS,
}

public enum DamageableType
{
	Standard = 0,
	Armour,
	Rubber,
	Volatile,
	Shield,
	Wood,
	Rock,
	Compound
}

public enum FactionType
{
	GSO = 1,
	GC,
	EXP, // also known as Reticule Research (RR)
    VEN,
	HE,
	SPE,
	BF,
	SJ,
}

