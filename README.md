## Щоденник розробника

### 24.09.2026 — Юніт 1: персонаж і рух

Створив спочатку нову сцену `MainScene` у теці `Scenes`. Наступний момент у методичці пропонував відкрити `Assets → _2DAdventureGame → Art` і знайти `PlayerCharacter`. Спочатку гуляв просто по папці `Character` та не міг знайти спрайт, але потім догадався тикнути по теці набору артів, точніше просто по папці `Ducko`. Перетягнув персонажа на сцену, виставив йому координати X = -2, Y = 0, а камеру залишив у (0,0).

Далі створив теку `Scripts` і перший скрипт `PlayerController`. Відкрив його через Visual Studio та написав перші рядки коду:

```csharp
void Update()
{
    Vector2 position = transform.position;
    position.x = position.x + 0.1f;
    transform.position = position;
}
Переписав ці рядки коду. Зрозумів, що Vector2 це тип даних, який зберігає x, y, transform — це посилання на компонент Transform, через який ми можемо маніпулювати з рухом.
Накинув скрипт на персонажа, тицнув Play і... персонаж просто миттєво зник за правим краєм екрана. Довго не міг зрозуміти, чому він так чеше, а виявилося, що метод Update() викликається кожен кадр. Якщо в мене редактор видає під декілька сотень FPS, то він додає по 0.1 одиниці сотні разів на секунду, от чувак і вилітає за кадр.
```
Далі почав збирати підлогу для рівня. Створив 2D Object → Tilemap → Rectangular (з'явилися Grid і Tilemap).
Потім у теці артів створив папку Tiles і новий Rule Tile з назвою FirstTile.
Трішки витратив часу на цей пункт: "Виділіть FirstTile і в Inspector перетягніть спрайт Tile1 (він у теці набору артів) у поле Default Sprite".
Намагався зрозуміти що до чого взагалі, потім шукав де знаходиться Default Sprite. Але розкопав, закинув туди спрайт.
Потім відкрив Tile Palette, створив Game Palette і перетягнув туди свої тайли (FirstTile і SecondTile).
Почав малювати підлогу пензлем у вікні Scene, і тут вилізла якась фігня: плитки були якісь дрібні, з проміжками між собою, та ще й малювалися поверх персонажа,
так що його взагалі не було видно.
Довелося лізти розбиратися. Виявилося, треба було виділити спрайти плиток у Project і змінити Pixels Per Unit з 100 на 64 (і не забути натиснути Apply).
А щоб персонаж не ховався під підлогою, у Tilemap Renderer виставив Order in Layer = -10 (бо у персонажа стоїть 0, тому те, що нижче, малюється позаду).
Після цього сітка стала рівненькою, а підлога лягла чітко під ноги.
Наступний великий блок — налаштування вводу через нову Input System. У скрипті додав using UnityEngine.InputSystem; і оголосив public InputAction MoveAction;.
Потім у Unity для цього поля виставив Action Type — Value, Control Type — Vector2. Додав композит Up/Down/Left/Right і налаштував через функцію Listen стрілочки на клавіатурі. Зробив ще один такий самий композит на WASD, а в пункті «More things to try» додав ще й лівий стік геймпада (Gamepad → Left Stick),
щоб уже точно з усього можна було керувати.
У коді ввімкнув дію через MoveAction.Enable() у Start(), а в Update() замінив рух на читання вводу: Vector2 move = MoveAction.ReadValue<Vector2>();.
Подивився в консоль — при натисканні стрілок туди виводяться координати. Помітив цікаву річ: коли тиснеш вгору і вправо одночасно, консоль показує не (1.00, 1.00), а (0.71, 0.71).
Виявилося, композит автоматично нормалізує вектор, щоб по діагоналі персонаж не бігав швидше, ніж по прямій.
Ну і фіналочка — зробити так, щоб швидкість не залежала від FPS. Спочатку зробив експеримент з методички: додав у Start() обмеження QualitySettings.vSyncCount = 0; Application.targetFrameRate = 10;.
Запустив — персонаж через ці 10 кадрів на секунду ледь повз як черепаха. Тоді переписав Update() остаточно:
C#
```csharp
void Update()
{
    Vector2 move = MoveAction.ReadValue<Vector2>();
    Debug.Log(move);

    Vector2 position = (Vector2)transform.position + move * 3.0f * Time.deltaTime;
    transform.position = position;
}

```
Зрозумів штуку про Time.deltaTime: це час у секундах між поточним і попереднім кадром. Коли ми множимо на нього, ми переводимо швидкість з «одиниць за кадр» в «одиниці за секунду».
Тепер значення 3.0f означає чітко 3 одиниці на секунду на будь-якому комп'ютері чи калькуляторі, хоч при 10 FPS (просто буде ривками йти), хоч при 240 FPS.

### 07.10.2026 — Юніт 2: оточення і фізика

**Що зроблено:**
* **План і тайлсет:** Накидав план рівня в зошиті (куди рухатися, де перешкоди й аптечки). В Unity у теці `Environment` виділив тайлсети `Floor...`, виставив `Sprite Mode = Multiple`, `Pixels Per Unit = 64`. У Sprite Editor нарізав їх сіткою 3х3 (`Grid By Cell Count`, C=3, R=3). Створив тайли в теці `Tiles` і через Tile Palette зафарбував рівень, лишивши бордюр на 2 клітинки за межами камери.
* **Декорації та сортування:** Закинув декорації (ящики, кущі). Щоб робот адекватно заходив за перешкоди, в `Renderer2D` виставив `Transparency Sort Mode = Custom Axis` (X=0, Y=1, Z=0). У спрайтів декорацій змінив `Pivot` на `Bottom`. Персонажу у Sprite Editor поставив `Custom Pivot` на ноги (X=0.5, Y=0) та в `Sprite Renderer` перемкнув `Sprite Sort Point` з `Center` на `Pivot`. Після цього перетягнув декорації у теку `Prefabs`.
* **Зона шкоди:** Додав `DamageZone`, у `Sprite Renderer` поставив `Draw Mode = Tiled` і `Tile Mode = Adaptive`, а в самому спрайті виставив `Mesh Type = Full Rect`. Тепер її можна розтягувати через Rect Tool без викривлення текстури.
* **Фізика персонажа й перешкод:** На префаб робота повісив `Rigidbody 2D` та `Box Collider 2D` (зменшив його до нижньої частини спрайта, суто під ноги). Додав колайдери на префаби декорацій. У `Rigidbody 2D` робота поставив `Gravity Scale = 0` та заморозив обертання по Z (`Constraints -> Freeze Rotation Z`).
* **Фізичний рух:** Переписав рух у `PlayerController.cs`: прибрав прямий сув `transform.position` з `Update()`, і переніс фізичне переміщення через `rigidbody2d.MovePosition()` у метод `FixedUpdate()`.
* **Колайдер тайлмапа:** На `Tilemap` додав `Tilemap Collider 2D` та `Composite Collider 2D`. На тайлах підлоги у теці `Tiles` (і в Rule Tile) змінив `Collider Type` на `None`. Для `Composite Collider 2D` виставив `Composite Operation = Merge`, а в `Rigidbody 2D` тайлмапа — `Body Type = Static`.

**Що не вийшло або забрало найбільше часу:**
* **Падіння в безодню:** Коли вперше додав `Rigidbody 2D` на робота й натиснув Play, чувак миттєво полетів униз і зник з екрана. Виявилося, Unity за замовчуванням вмикає гравітацію для dynamic-тіл, а в грі з виглядом зверху падати нікуди. Поставив `Gravity Scale = 0` і застосував зміни до префабу через `Overrides -> Apply All`.
* **Тремтіння біля ящика та розвертання:** Підбігаю до ящика — робот зачіпає кут і починає крутитися як божевільний, а впритул до стіни дрібно тремтить. Поворот полікував галочкою `Freeze Rotation Z`. А от із тремтінням довелося розбиратися в коді: виявилося, що переміщення через `transform.position` силоміць заштовхує робота в колайдер, а фізичний рушій на наступному кадрі помічає перетин і виштовхує його назад.
* **Застрягання на підлозі:** Після додавання `Tilemap Collider 2D` робот взагалі не міг ступити й кроку. Виявилося, що за замовчуванням колайдер з'явився на КОВЖНІЙ плитці, включаючи звичайну підлогу. Довелося виділяти всі проходимі тайли та перемикати `Collider Type = None`.

**Що зрозумів про FixedUpdate і колайдери:**
* `Update()` викликається кожен кадр (частота залежить від FPS), тому там ми лише зчитуємо ввід гравця (`MoveAction.ReadValue<Vector2>()`). А `FixedUpdate()` викликається з чітким фіксованим інтервалом (50 разів на секунду) у такт фізичному рушію Unity.
* `transform.position = ...` просто телепортує об'єкт в координату в обхід фізики. Через це робот опинявся всередині ящика, і движок його жорстко виштовхував. Метод `rigidbody2d.MovePosition()` каже рушію: «прорахуй шлях сюди руками фізики з урахуванням усіх колайдерів», тому робот м'яко зупиняється й ковзає уздовж стін без тремтіння.
* `Composite Collider 2D` з операцією `Merge` зливає безліч окремих квадратиків тайлів води в один суцільний контур. Це полегшує роботу рушію й прибирає мікростики, за які персонаж раніше міг перечіплятися.
* `Body Type = Static` підказує Unity, що цей об'єкт (рівень) нікуди не рухатиметься, тому фізичний рушій взагалі не витрачає ресурси на розрахунок його динаміки.

**Код скрипта PlayerController:**
```csharp
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public InputAction MoveAction;
    
    Rigidbody2D rigidbody2d;
    Vector2 move;

    void Start()
    {
        MoveAction.Enable();
        rigidbody2d = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        move = MoveAction.ReadValue<Vector2>();
        Debug.Log(move);
    }

    void FixedUpdate()
    {
        Vector2 position = (Vector2)rigidbody2d.position + move * 3.0f * Time.deltaTime;
        rigidbody2d.MovePosition(position);
    }
}
```

### 21.10.2026 — Юніт 4: вороги і снаряди

**Що зроблено:**
* **Ворог:** У `Art → Ducko → Characters` виділив спрайт `Enemy`, у Sprite Editor поставив `Custom Pivot` (X=0.5, Y=0) — так само, як роботу в лабі 3. Закинув на сцену, `Sprite Sort Point = Pivot`, додав `Box Collider 2D` з галочкою `Is Trigger` і `Rigidbody 2D` (`Gravity Scale = 0`, `Freeze Rotation Z`). Повісив скрипт `EnemyController` і зробив префаб `Enemy`.
* **Патруль:** У `EnemyController` рух іде через `rigidbody2d.MovePosition()` у `FixedUpdate()`, галочка `vertical` перемикає вісь, а в `Update()` крутиться таймер: як тільки `timer < 0`, робимо `direction = -direction` і заводимо таймер знову. Шкоду ворог робить в `OnTriggerEnter2D` через той самий `ChangeHealth(-1)`, тому невразливість з лаби 4 працює і тут.
* **Анімація ворога:** Додав на префаб `Animator`, створив контролер `Enemy` у `Animation → Ducko → Enemy → AnimationClips`. `EnemySheet` перевів на `Pixels Per Unit = 100`, бо стояло 150 і кадри були б меншими за ворога на сцені. Кліп `EnemyLeft` зібрав з `GolemBoilerWalkSide1…4`, `Samples = 4`. `EnemyRight` — ті самі кадри плюс `Sprite Renderer.Flip X` на кадрах 0 і 4.
* **Blend Tree:** Видалив стани `EnemyLeft`/`EnemyRight`, створив Blend Tree з типом `2D Simple Directional`, параметри `Move X` і `Move Y`. Розставив кліпи: Left (-0.5, 0), Right (0.5, 0), Down (0, -0.5), Up (0, 0.5). У скрипті в кожній гілці `if (vertical)` / `else` передаю `SetFloat("Move X", ...)` і `SetFloat("Move Y", ...)`.
* **Анімації робота:** На `PlayerCharacter` додав `Animator` з готовим контролером `Player`. У `PlayerController` з'явились `animator` і `moveDirection`, в `Update()` передаю `Look X`, `Look Y`, `Speed`, а в `ChangeHealth` при ударі — `SetTrigger("Hit")`.
* **Снаряд:** Спрайт `Projectile` з теки `VFX` (`Pixels Per Unit = 300`), `Box Collider 2D` з `Is Trigger`, `Rigidbody 2D` з `Gravity Scale = 0`, скрипт `Projectile`, префаб. У `PlayerController` додав `projectilePrefab`, `LaunchAction` (`Action Type = Button`, прив'язка `C [Keyboard]` через Listen) і функцію `Launch()` з `Instantiate` та `AddForce`.
* **Шари:** Додав шари `Character` (робот) і `Projectiles` (префаб снаряда), у `Physics 2D → Layer Collision Matrix` зняв галочку на їх перетині.
* **Лагодження:** У ворога з'явились `broken` і `Fix()` (`broken = false`, `rigidbody2d.simulated = false`, `SetTrigger("Fixed")`). Снаряд в `OnTriggerEnter2D` шукає `EnemyController` і викликає `Fix()`. Зробив кліп `EnemyFixed`, перехід з Blend Tree з умовою-тригером `Fixed` без `Has Exit Time`. Снаряди, що полетіли в нікуди, знищуються, коли `transform.position.magnitude > 100`.

**Що не вийшло або забрало найбільше часу:**
* **Зламаний ворог зі старої спроби:** Після відкату комітів у сцені лишився `Enemy` червоного кольору — префаба нема, скрипта нема, в контролері `Missing`, а в консолі `The referenced script (Unknown) on this Behaviour is missing!`. Лагодити не було сенсу, просто видалив його і пройшов юніт з першого кроку.
* **Flip X на кадрі 4:** Поставив галочку `Flip X` на кадрі 0, перейшов на кадр 4 — галочка начебто стоїть, але ключа (ромбика) там нема, значення просто тягнеться з кадру 0. Довелося правою кнопкою по рядку `Flip X` на кадрі 4 → `Add Key`.
* **Параметр `Blend` у Blend Tree:** Перейменував `Blend` на `Move X`, переключив тип на `2D Simple Directional`, а в другому полі Parameters так і лишився `Blend`, якого вже не існує. Вручну вибрав там `Move Y`.
* **Два скрипти `Projectile`:** В `Add Component` на пошук `Projectile` виїхало два варіанти — мій і `Projectile (Beginner2D)` з `Tutorial_Demo`. Якби взяв другий, снаряд би не лагодив ворога. Взяв перший, без підпису.
* **Нема кадрів Fixed1…Fixed4:** У наборі Ducko кадри полагодженого ворога називаються `GolemBoilerFixedDown/Side/Up` по 5 штук. Для `EnemyFixed` взяв `GolemBoilerFixedDown1…4`, бо полагоджений ворог стоїть обличчям до камери.

**Що зрозумів про Animator, шари й Awake:**
* Вікно `Animation` — це кадри одного кліпу на шкалі часу, а `Animator` — це схема станів і переходів. Blend Tree сам вибирає, який кліп грати, за числами `Move X` і `Move Y`, які я передаю зі скрипта через `SetFloat`. Ворог рухається тільки по одній осі, тому одна координата завжди 0, а друга — `direction` (1 або -1).
* `Trigger` в аніматорі — це як кнопка: скрипт її натискає (`SetTrigger("Hit")`, `"Launch"`, `"Fixed"`), перехід спрацьовує, і кнопка сама відпускається. З `Is Trigger` у колайдера нічого спільного, просто слово те саме.
* `Awake()` замість `Start()` у снаряда: `Instantiate` одразу викликає `Awake()`, а `Start()` — тільки пізніше, вже після того, як `PlayerController` викликав `Launch()`. Якби `rigidbody2d` брався в `Start()`, на момент `AddForce` він був би `null` і вилітав би `NullReferenceException`.
* Снаряд народжується прямо всередині колайдера робота, тому без шарів він одразу ловив `OnTriggerEnter2D` з роботом і знищувався. Шари з вимкненою парою `Character × Projectiles` — це фільтр: фізика просто не перевіряє цю пару. І це зберігається в `TagManager.asset` та `Physics2DSettings.asset`, тому їх обов'язково треба комітити, інакше в клоні проєкту снаряд знову зникатиме.

**Які Speed і Change Time обрав і для якого гравця:**
* `Speed = 1`, `Change Time = 3`, `Vertical` увімкнено. Ворог стоїть у (0, -4) і ходить відрізком у 3 одиниці (Speed × Change Time) вгору до -1 і назад — якраз між зоною шкоди внизу зліва і шоколадною зоною справа, тому маршрут лежить на вільному місці.
* Це під гравця, який тільки знайомиться з грою: ворог повільний, є час побачити, куди він іде, і обійти його. Снаряд летить 6 одиниць на секунду (300 × 0.02 на масу 1) — у шість разів швидше за ворога, тому влучити легко. Для складнішого місця можна поставити другого ворога зі `Speed = 2` і `Change Time = 1.5` — маршрут той самий, а реагувати треба вдвічі швидше.

**Що робить мій снаряд з ворогом і як це пасує до моєї рамки:**
* Снаряд не знищує ворога, а лагодить: ворог зупиняється, перестає шкодити, снаряди пролітають крізь нього, а сам він починає світитися зеленим (`EnemyFixed`). Мої вороги — це зламані бойлери-големи, а качка в кашкеті — механік, тож логічно, що вона їх ремонтує, а не б'є. Світ від цього не злий, а просто зіпсований, і задача гравця — повернути його до ладу.

**Що робитиму далі:**
* Пункти «Спробувати більше»: швидкий і повільний варіант ворога та обмеження темпу пострілів таймером, як у невразливості.
* Юніт 5: NPC з діалогом і перемога, коли полагоджено всіх ворогів.

**Код скрипта EnemyController:**
```csharp
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float speed;
    public bool vertical;
    public float changeTime = 3.0f;

    Rigidbody2D rigidbody2d;
    Animator animator;
    float timer;
    int direction = 1;
    bool broken = true;

    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        timer = changeTime;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer < 0)
        {
            direction = -direction;
            timer = changeTime;
        }
    }

    void FixedUpdate()
    {
        if (!broken)
        {
            return;
        }

        Vector2 position = rigidbody2d.position;

        if (vertical)
        {
            position.y = position.y + speed * direction * Time.deltaTime;
            animator.SetFloat("Move X", 0);
            animator.SetFloat("Move Y", direction);
        }
        else
        {
            position.x = position.x + speed * direction * Time.deltaTime;
            animator.SetFloat("Move X", direction);
            animator.SetFloat("Move Y", 0);
        }

        rigidbody2d.MovePosition(position);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.gameObject.GetComponent<PlayerController>();

        if (player != null)
        {
            player.ChangeHealth(-1);
        }
    }

    public void Fix()
    {
        broken = false;
        rigidbody2d.simulated = false;
        animator.SetTrigger("Fixed");
    }
}
```

### 05.11.2026 — Юніт 5: HUD, діалоги, ігровий цикл

**Що зроблено:**
* **HUD:** У теці `UI` створив `GameUI.uxml`. Зверху `HealthBarBackground` з рамкою `UIHealthFrame`, всередині `HealthBarContainer` з `HealthBar` (`UIHealthBar`, `stretch-to-fill`) і `CharacterPortrait` останнім у списку, щоб портрет малювався поверх смуги. На об'єкті `UIDocument` в сцені — `Panel Settings` з `Scale Mode = Scale With Screen Size`, `Reference Resolution = 1920 × 1080`.
* **UIHandler як синглтон:** `public static UIHandler instance { get; private set; }` заповнюється в `Awake()`. `SetHealthValue(float)` міняє `style.width` смуги через `Length.Percent`. У `PlayerController.ChangeHealth` замість `Debug.Log` тепер `UIHandler.instance.SetHealthValue(currentHealth / (float)maxHealth)`.
* **NPC:** `NonPlayerCharacterSheet` перевів у `Sprite Mode = Multiple` (нарізка `MarthaIdle1…4` 256×256 уже була). Зробив кліп `NPC` з `MarthaIdle1…3` (`Samples = 4`, Loop Time) і контролер у `Animation → Ducko → NPC`. Об'єкт `NPC` на новому шарі `NPC`, `Box Collider 2D` під ноги, дочірній `DialogueBox` з бульбашкою «Press X», яку `NonPlayerCharacter` ховає в `Start()`. Зберіг у префаб `NPC`.
* **Розмова:** У `PlayerController` додав `TalkAction` (`Button`, `X [Keyboard]`). В `Update()` кожен кадр пускаю `Physics2D.Raycast` з точки трохи вище ніг у бік `moveDirection` на 1.5 одиниці лише по шару `NPC`. Влучив — вмикаю бульбашку і запам'ятовую NPC в `lastNonPlayerCharacter`; натиснув X — `UIHandler.instance.DisplayDialogue()` показує вікно `NPCDialogue` на 4 секунди. Відвернувся — бульбашка гасне.
* **Ігровий цикл:** `EndScreen.uxml` з `LoseScreenContainer` і `WinScreenContainer` (`opacity: 0`, `transition-duration: 1s`). `GameManager` на окремому об'єкті: здоров'я 0 — `DisplayLoseScreen()`, всі `EnemyController` з `isBroken == false` — `DisplayWinScreen()`, і через `Invoke(nameof(ReloadScene), 3f)` сцена завантажується знову. В `EnemyController` для цього з'явилась властивість `isBroken`.
* **Спробувати більше (2 пункти):** прапорець `bool gameOver` у `GameManager` — на початку `Update()` `if (gameOver) return;`, а перед `Invoke` — `gameOver = true;`. І лічильник «Полагоджено 0 / 1» у правому верхньому куті: `Label` у `GameUI`, `UIHandler` рахує ворогів у `Start()`, а `Fix()` ворога викликає `UIHandler.instance.EnemyFixed()`.
* **Перевірка в Play:** смуга меншає на зоні шкоди, бульбашка з'являється тільки обличчям до NPC, X показує репліку, після влучання у ворога — екран перемоги, після 0 здоров'я — поразка, в обох випадках рівень перезапускається. Console без помилок і попереджень.

**Що не вийшло або забрало найбільше часу:**
* **NPC не нарізався:** спрайт-шит NPC стояв у `Sprite Mode = Single`, хоча кадри `MarthaIdle1…4` в метаданих уже були, тому в Project показувалась одна картинка 512×512 з чотирма качками. Перемкнув на `Multiple`, і кадри з'явилися.
* **NPC закрив аптечку:** спершу поставив NPC у (-3.5, 2.2), а бульбашка «Press X» налізла прямо на аптечку в лівому верхньому куті. Переставив його в (-4.3, -0.2) — на одну лінію з точкою старту робота, тепер досить пройти ліворуч, і промінь його ловить.
* **Дублікати класів:** в `Tutorial_Demo/Demo_Scripts` лежать свої `UIHandler`, `NonPlayerCharacter`, `GameManager` у просторі імен `Beginner2D`. В `Add Component` кожен виїжджає двічі — брав свій, без підпису, як і з `Projectile` в юніті 4.
* **Перезапуск сцени:** у `Build Profiles → Scene List` були тільки демо-сцени (`StartScreen`, `Ruby_Example_Scene` тощо), а моєї `MainScene` не було — `SceneManager.LoadScene` її б не знайшов. Додав через `Add Open Scenes`.

**Що зрозумів про UI Toolkit, синглтон і raycast:**
* В UI Toolkit інтерфейс — це дерево `VisualElement` у файлі `.uxml`, а стиль (розмір, картинка, прозорість) пишеться прямо на елементі. Скрипт шукає елемент через `rootVisualElement.Q<VisualElement>("HealthBar")` за ім'ям, і регістр важливий: `Healthbar` і `HealthBar` — це різні імена. Порядок у дереві = порядок малювання: хто нижче, той зверху.
* Синглтон — клас, у якого в грі рівно один екземпляр, і до нього можна достукатись звідусіль через статичну властивість. Робот, ворог і `GameManager` просто пишуть `UIHandler.instance`, без поля в Inspector. Для ворогів так не можна, бо їх кілька, і `instance` перезаписувався б останнім.
* Промінь замість тригера, бо тригер спрацьовує від будь-якого дотику, навіть спиною, а промінь іде саме туди, куди дивиться робот. Маска `LayerMask.GetMask("NPC")` пропускає все інше — і ворогів, і декорації.
* `(float)` у `currentHealth / (float)maxHealth`: ділення двох `int` у C# відкидає дробову частину, тож 4 / 5 дало б 0, і смуга була б або повна, або порожня. Якщо хоча б одне число `float`, результат 0.8 — 80% ширини.
* `Invoke` без `gameOver` ставив би новий виклик `ReloadScene` у чергу щокадру, поки йдуть 3 секунди, — прапорець робить кінець гри одноразовою подією.

**Що каже мій NPC і чому саме це:**
* «Тисни C, щоб стріляти шестернями, і допоможи полагодити ці зламані бойлери! Повертайся, коли закінчиш». Мої вороги — зламані бойлери-големи, а NPC — механік, тому вона одразу пояснює і керування (C), і мету (полагодити, а не знищити). Гравець, який першим ділом іде до неї, вже знає, що робити.

**Умови перемоги й поразки в моєму зрізі:**
* Перемога — полагоджено всіх ворогів (зараз один бойлер), лічильник показує «1 / 1». Поразка — здоров'я падає до 0 на зоні шкоди чи від ударів ворога. Через 3 секунди після екрана рівень починається спочатку, з повним здоров'ям.

**Що робитиму далі:**
* Поставити ще 2 ворогів, щоб лічильник був «0 / 3», і другого NPC зі своєю реплікою.
* Юніт 6: камера за роботом, звуки і візуальні ефекти.

**Код скрипта UIHandler:**
```csharp
using UnityEngine;
using UnityEngine.UIElements;

public class UIHandler : MonoBehaviour
{
    public static UIHandler instance { get; private set; }

    public float displayTime = 4.0f;
    private VisualElement m_NonPlayerDialogue;
    private float m_TimerDisplay;

    private VisualElement m_Healthbar;
    private VisualElement m_WinScreen;
    private VisualElement m_LoseScreen;

    // Спробувати більше: лічильник полагоджених ворогів
    private Label m_RobotCounter;
    private int m_EnemiesTotal;
    private int m_EnemiesFixed;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        UIDocument uiDocument = GetComponent<UIDocument>();

        m_Healthbar = uiDocument.rootVisualElement.Q<VisualElement>("HealthBar");
        SetHealthValue(1.0f);

        m_NonPlayerDialogue = uiDocument.rootVisualElement.Q<VisualElement>("NPCDialogue");
        m_NonPlayerDialogue.style.display = DisplayStyle.None;
        m_TimerDisplay = -1.0f;

        m_LoseScreen = uiDocument.rootVisualElement.Q<VisualElement>("LoseScreenContainer");
        m_WinScreen = uiDocument.rootVisualElement.Q<VisualElement>("WinScreenContainer");

        m_RobotCounter = uiDocument.rootVisualElement.Q<Label>("RobotCounter");
        m_EnemiesTotal = FindObjectsByType<EnemyController>(FindObjectsSortMode.None).Length;
        UpdateRobotCounter();
    }

    public void SetHealthValue(float percentage)
    {
        m_Healthbar.style.width = Length.Percent(100 * percentage);
    }

    private void Update()
    {
        if (m_TimerDisplay > 0)
        {
            m_TimerDisplay -= Time.deltaTime;
            if (m_TimerDisplay < 0)
            {
                m_NonPlayerDialogue.style.display = DisplayStyle.None;
            }
        }
    }

    public void DisplayDialogue()
    {
        m_NonPlayerDialogue.style.display = DisplayStyle.Flex;
        m_TimerDisplay = displayTime;
    }

    public void DisplayWinScreen()
    {
        m_WinScreen.style.opacity = 1.0f;
    }

    public void DisplayLoseScreen()
    {
        m_LoseScreen.style.opacity = 1.0f;
    }

    public void EnemyFixed()
    {
        m_EnemiesFixed++;
        UpdateRobotCounter();
    }

    void UpdateRobotCounter()
    {
        m_RobotCounter.text = "Полагоджено " + m_EnemiesFixed + " / " + m_EnemiesTotal;
    }
}
```
