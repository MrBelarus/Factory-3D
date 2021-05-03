using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementController : MonoBehaviour
{
    public static AchievementController instance;

    [Header("UI elements and it's setup")]
    [SerializeField] private GameObject achievementObjWithAnimation;
    [SerializeField] private Text title;
    [SerializeField] private Text description;
    [SerializeField] private AnimationClip showAnimationClip;
    private bool animationIsPlaying;
    private Queue<Achievement> showAchievementQueue = new Queue<Achievement>();

    [Header("Achievements menu setup")]
    [SerializeField] private RectTransform content;
    [SerializeField] private Color starColorAchieved;
    [SerializeField] private Color starBackgroundColorAchieved;
    [SerializeField] private RectTransform achievementMenuPrefabTransform;
    [SerializeField] private float contentTopOffsetFirstItem;
    [SerializeField] private float offsetBetweenAchievements;
    [SerializeField] private string titleParentObjName;
    [SerializeField] private string descriptionParentObjName;
    [SerializeField] private string starImageParentObjName;
    private Image[] starBackgroundImages;    //the both have the same order
    private Image[] starImages;              //as achievements array below
    private float achievementObjSizeY;
    private bool allAchievementsForMenuWereLoaded = false;

    public Achievement[] achievements;
    private SaveSystem saveSystem;
    private GameUIManager gameUIManager;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        saveSystem = SaveSystem.instance;

        if (saveSystem.Data.achievements == null)   //new save -> new achievements data
        {
            MoneyCountAchievement moneyCountFifthStage = new MoneyCountAchievement(null, "It's time to stop!", "You have 500.000$ on your account!", 500_000, false);
            MoneyCountAchievement moneyCountFourthStage = new MoneyCountAchievement(moneyCountFifthStage, "Sheikh!", "You have 100.000$ on your account!", 100_000, false);
            MoneyCountAchievement moneyCountThirdStage = new MoneyCountAchievement(moneyCountFourthStage, "Abramovich's Cousin", "You have 50.000$ on your account!", 50_000, false);
            MoneyCountAchievement moneyCountSecondStage = new MoneyCountAchievement(moneyCountThirdStage, "Business is growing up!", "You have 25.000$ on your account!", 25_000, false);

            achievements = new Achievement[]
            {
                //Purchaser Achievements
                new PurchaserBuyAchievement(Materials.IronOre, "Iron Man", "You have bought 100+ iron ores in the Purchaser", 100),
                new PurchaserBuyAchievement(Materials.Sand, "The Sandman", "You have bought 100+ Sand Objects in the Purchaser", 100),
                new PurchaserBuyAchievement(Materials.Plastic, "Sounds not ecological", "You have bought 100+ Plastic Objects in the Purchaser", 100),
                new PurchaserBuyAchievement(Materials.Rubber, "I Love Rubber!", "You have bought 100+ Rubber Objects in the Purchaser", 100),
                new PurchaserBuyAchievement(Materials.Water, "Water well!", "You have bought 100+ Water Objects in the Purchaser", 100),
                
                //Factory Achievements
                new FactoryProduceAchievement(Materials.Car,"Car producer", "You have produced 100+ Cars!", 100),
                new FactoryProduceAchievement(Materials.Bottle,"Should I fill it?", "You have produced 100+ Bottles!", 100),
                new FactoryProduceAchievement(Materials.Clock,"What's the time?", "You have produced 100+ Clocks!", 100),
                new FactoryProduceAchievement(Materials.Gear,"Engineer", "You have produced 100+ Gears!", 100),
                new FactoryProduceAchievement(Materials.Glass,"Glazier", "You have produced 100+ Glasses!", 100),
                new FactoryProduceAchievement(Materials.GlassBottle,"Where is my message in the bottle", "You have produced 100+ Glass Bottles!", 100),
                new FactoryProduceAchievement(Materials.Metal,"WE NEED MORE METAL!", "You have produced 100+ Metals!", 100),
                new FactoryProduceAchievement(Materials.Soda_Glass,"Real classic", "You have produced 100+ Glass Soda objects!", 100),
                new FactoryProduceAchievement(Materials.Soda_Plastic,"Holiday is coming", "You have produced 100+ Plastic Soda objects!", 100),
                new FactoryProduceAchievement(Materials.Toy,"Toys story", "You have produced 100+ Toys!", 100),

                //Keypress Achievements
                new KeyboardAchievement(KeyCode.F, "Pay respect", "Press F quickly 25 times!", 25),

                //Factory Obj Buy Achievements
                new FactoryObjBuyAchievement(FactoryObjTypes.Pipeline, "Henry Ford's successor", "You have bought 100+ Pipelines", 100),
                new FactoryObjBuyAchievement(FactoryObjTypes.Factory, "Industrial King", "You have bought 50+ Factories", 50),
                new FactoryObjBuyAchievement(FactoryObjTypes.Purchaser, "Kontrol'naya zakupka", "You have bought 50+ Purchasers", 50),
                new FactoryObjBuyAchievement(FactoryObjTypes.SellPort, "Successful business", "You have bought 50+ Sellers", 50),

                //Factory Obj Deleted Achievements
                new FactoryObjDeleteAchievement(FactoryObjTypes.Factory, "Seek&Destroy factories", "You have deleted 50+ Factories", 50),
                new FactoryObjDeleteAchievement(FactoryObjTypes.Pipeline, "Seek&Destroy pipelines", "You have deleted 50+ Pipelines", 50),
                new FactoryObjDeleteAchievement(null, "I just like destroy..", "You have deleted 100+ Factory objects " +
                "[Pipelines, Sellers, Purchasers or Factories]", 100),

                //Money Achievements
                new MoneyCountAchievement(moneyCountSecondStage, "First pocket money", "You have 10.000$ on your account!", 10_000, true),
                moneyCountSecondStage,
                moneyCountThirdStage,
                moneyCountFourthStage,
                moneyCountFifthStage,
            };
        }
        else                                        //existing save -> load achievements data
        {
            achievements = saveSystem.Data.achievements;
        }

        SetupSubscriptionOfAchievements();
    }

    private void Start()
    {
        gameUIManager = GameUIManager.instance;
    }

    private void SetupSubscriptionOfAchievements()
    {
        foreach (Achievement achievement in achievements)
        {
            if (achievement.subscribeOnLoad && !achievement.achieved)
            {
                achievement.SubscribeEvent();
            }
        }
    }

    public void SetupAchievementsMenu()
    {
        if (allAchievementsForMenuWereLoaded)
        {
            for (int i = 0; i < achievements.Length; i++)
            {
                if (achievements[i].achieved && starImages[i].color != starColorAchieved)
                {
                    starImages[i].color = starColorAchieved;
                    starBackgroundImages[i].color = starBackgroundColorAchieved;
                }
            }
        }
        else
        {
            achievementObjSizeY = achievementMenuPrefabTransform.sizeDelta.y;
            starBackgroundImages = new Image[achievements.Length];
            starImages = new Image[achievements.Length];

            //calculating content size
            content.sizeDelta = new Vector2(content.sizeDelta.x,
                contentTopOffsetFirstItem + (achievementObjSizeY + offsetBetweenAchievements) * achievements.Length);

            for (int i = 0; i < achievements.Length; i++)
            {
                GameObject achievementObj = Instantiate(achievementMenuPrefabTransform.gameObject, content);

                RectTransform rectTransform = achievementObj.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(0f,
                    -(contentTopOffsetFirstItem + achievementObjSizeY / 2 + i * (achievementObjSizeY + offsetBetweenAchievements)));

                achievementObj.transform.Find(descriptionParentObjName).GetChild(0).GetComponent<Text>().text = achievements[i].description;
                achievementObj.transform.Find(titleParentObjName).GetChild(0).GetComponent<Text>().text = achievements[i].title;

                Image background = achievementObj.transform.Find(starImageParentObjName).GetComponent<Image>();
                Image star = achievementObj.transform.Find(starImageParentObjName).GetChild(0).GetComponent<Image>();

                if (achievements[i].achieved)
                {
                    star.color = starColorAchieved;
                    background.color = starBackgroundColorAchieved;
                }

                starBackgroundImages[i] = background;
                starImages[i] = star;
            }

            allAchievementsForMenuWereLoaded = true;
        }
    }

    public void ShowAchievement(Achievement achievement)
    {
        if (animationIsPlaying)
        {
            showAchievementQueue.Enqueue(achievement);
            return;
        }

        title.text = achievement.title;
        description.text = achievement.description;

        if (gameUIManager && gameUIManager.IsAchievementMenuActive)
        {
            //update achievements menu if it's active
            SetupAchievementsMenu();
        }

        if (AudioManager.instance)
        {
            AudioManager.instance.PlaySound(Sounds.AchievementRecieved);
        }

        achievementObjWithAnimation.SetActive(true);
        animationIsPlaying = true;

        StartCoroutine(HideAchievementObjWithDelay(showAnimationClip.length + 0.05f));
    }

    IEnumerator HideAchievementObjWithDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        achievementObjWithAnimation.SetActive(false);
        animationIsPlaying = false;

        if (showAchievementQueue.Count > 0)
        {
            ShowAchievement(showAchievementQueue.Dequeue());
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();

        foreach(Achievement achievement in achievements)
        {
            achievement.UnsubscribeEvent();
        }
    }
}
