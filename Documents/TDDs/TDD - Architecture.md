___
**Game:** Wander
**Author:** DukTofn 
**Last Updated:** 23/07/2026 
___

## Mục lục

0. [Tóm tắt điều chỉnh so với bản gốc](https://claude.ai/chat/e9cac3af-35cb-4f53-8b57-db1ed741f3c1#0-t%C3%B3m-t%E1%BA%AFt-%C4%91i%E1%BB%81u-ch%E1%BB%89nh-so-v%E1%BB%9Bi-b%E1%BA%A3n-g%E1%BB%91c)
1. [Bảng phân lớp toàn bộ hệ thống theo GDD](https://claude.ai/chat/e9cac3af-35cb-4f53-8b57-db1ed741f3c1#1-b%E1%BA%A3ng-ph%C3%A2n-l%E1%BB%9Bp-to%C3%A0n-b%E1%BB%99-h%E1%BB%87-th%E1%BB%91ng-theo-gdd)
2. [Assembly Reference Graph chính xác](https://claude.ai/chat/e9cac3af-35cb-4f53-8b57-db1ed741f3c1#2-assembly-reference-graph-ch%C3%ADnh-x%C3%A1c)
3. [Quy tắc giao tiếp giữa các layer](https://claude.ai/chat/e9cac3af-35cb-4f53-8b57-db1ed741f3c1#3-quy-t%E1%BA%AFc-giao-ti%E1%BA%BFp-gi%E1%BB%AFa-c%C3%A1c-layer)
4. [Thứ tự code](https://claude.ai/chat/e9cac3af-35cb-4f53-8b57-db1ed741f3c1#4-th%E1%BB%A9-t%E1%BB%B1-code)
5. [Checklist trước khi bắt đầu Phase 0](https://claude.ai/chat/e9cac3af-35cb-4f53-8b57-db1ed741f3c1#5-checklist-tr%C6%B0%E1%BB%9Bc-khi-b%E1%BA%AFt-%C4%91%E1%BA%A7u-phase-0)
6. [Folder Structure chi tiết](https://claude.ai/chat/e9cac3af-35cb-4f53-8b57-db1ed741f3c1#6-folder-structure-chi-ti%E1%BA%BFt)
7. [Đặc tả chi tiết từng Class](https://claude.ai/chat/e9cac3af-35cb-4f53-8b57-db1ed741f3c1#7-%C4%91%E1%BA%B7c-t%E1%BA%A3-chi-ti%E1%BA%BFt-t%E1%BB%ABng-class)

---

# 0. Tóm tắt điều chỉnh so với bản gốc

|#|Vấn đề trong bản gốc|Điều chỉnh|
|---|---|---|
|1|Domain "KHÔNG reference UnityEngine" nhưng code ví dụ dùng `Mathf.Pow` — 2 chỗ này mâu thuẫn nhau về mặt asmdef|Domain dùng `System.Math`/`double` thuần, không dùng `Mathf`. Giữ được compile-time guarantee thật sự|
|2|"Domain raise event qua interface, không tham chiếu SO" — chưa nói rõ cơ chế|Domain expose 1 `DomainEventBus` thuần C# (plain `event Action<T>`). Controller subscribe bus này rồi forward sang SO Event Channel|
|3|Không có `WW.Controllers.asmdef` dù mục 5/6 bản gốc đã ngầm định có layer này|Thêm asmdef thứ 5. Nếu không, Controller code nằm lẫn trong Presentation → Presentation gián tiếp có full quyền vào Data+Domain|
|4|"Presentation đọc Domain read-only" không compiler-enforced được|Đưa ra 2 mức nghiêm ngặt, khuyến nghị mức chặt cho hệ thống hay đổi nhiều (combat), mức lỏng cho phần ít rủi ro|
|5|`WisdomSlotConfig` chưa rõ nằm ở đâu|Đứng SO riêng ở `Data/`, không lồng trong `CombatBalanceConfig`|
|6|Enemy behavior pattern (GDD note "tài liệu riêng, chưa có") chưa có chỗ đứng trong kiến trúc|Đặt sẵn `IEnemyBehaviorStrategy` (Strategy pattern) rỗng ngay từ Phase 3|
|7|Chưa có chiến lược test theo layer, dù mục tiêu là "test nhanh"|Domain = EditMode test (không cần scene, nhanh nhất). Controller/Presentation = Play Mode / test thủ công|
|8|`ComboRule` "load từ Data Layer" nhưng chưa nói gom ở đâu|Gom vào 1 `ComboRuleDatabase` SO duy nhất, giống cách `SpellDefinition` được quản lý theo danh sách|
|9|Sơ đồ ASCII gốc (Presentation → Event → Domain → Data) dễ hiểu nhầm là reference graph thật|Đây là luồng dữ liệu khái niệm, không phải sơ đồ reference asmdef — xem bảng thật ở mục 2|

---

# 1. Bảng phân lớp toàn bộ hệ thống theo GDD

## 1.1. Data Layer — `WW.Data` (ScriptableObject, designer chỉnh trực tiếp trên Inspector)

| Asset / Class                                                                                       | Field chính                                                                                                                                                                                           | Nguồn GDD                                  |
| --------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------ |
| `CombatBalanceConfig` (nested: `HpConfig`, `MpConfig`, `ResistanceConfig`, `EffectModifiersConfig`) | `HP_CAP`, `HP_HALF`, `MP_COEFF`, `BASE_MP_RECOVERY`, exponent `p`, % của Enrage/Refreshing/Fortified/Energized/Overdrive/Crystalize/Burn/Drenched/Chilled/Dazed/Detonates/Distracted                  | Combat Design §2.2.2, §3, §6               |
| `ProgressionBalanceConfig` (nested: `ShopPriceConfig`, `RewardConfig`, `EventPoolConfig`)           | Giá theo Rank, Gold reward theo loại combat, tỷ lệ Rank Shop/Reward theo Arc, tỷ lệ + giá trị 10 Event                                                                                                | Progression Design §4, §5, §6              |
| `WisdomSlotConfig`                                                                                  | Ngưỡng WIS mở từng Spell Slot (1→5)                                                                                                                                                                   | Game Overview §2.1.3                       |
| `SpellDefinition`                                                                                   | `element`, `rank`, `manaCost`, `scalingWeights{POT,SPI,WIS,VIT}`, `appliedEffects[]`, `minWisdomToImprint`                                                                                            | Game Overview §2.1.1; Combat Design §2.2.1 |
| `EquipmentDefinition`                                                                               | `slotType` (Staff/Ring/Book/Garb), `rank`, `attributeBonuses[]`, `passiveEffect`                                                                                                                      | Game Overview §2.1.6/§2.1.7                |
| `RuneDefinition`                                                                                    | `rank`, `passiveEffect`                                                                                                                                                                               | Game Overview §2.1.8                       |
| `EffectDefinition`                                                                                  | `effectType` enum (Enrage, Refreshing, Fortified, Energized, Burn, Drenched, Chilled, Dazed, Regen, Armor, Distracted), `resolveTime` (Start/End/Instant), `duration`, `magnitude`, `isSelfStackable` | Combat Design §6                           |
| `ComboRuleDatabase` (list `ComboRule{requiredA, requiredB, resultCombo}`)                           | Overdrive, Crystalize, Detonates, Frozen                                                                                                                                                              | Game Overview §2.2.1; Combat Design §6     |
| `EnemyDefinition`                                                                                   | `enemyType` (Minion/Elite/Boss), `maxHp`, `potencies[4]`, `resistances[4]`, `spells[]`, `behaviorPattern` (enum, placeholder chờ Enemy Design)                                                        | Combat Design §4                           |
| `NodeDefinition`                                                                                    | `nodeType` (Combat-Normal/Elite/Boss, Camp, Merchant, Event, Town)                                                                                                                                    | Game Overview §2.1.12                      |
| `ArcMapConfig` (1 asset / Arc)                                                                      | `totalNodes`, `minNodesToEnd`, `minElite`, `minRest`, `minShop`, `minEvent`, `optionalBossCount`, `branchMin/Max`                                                                                     | Progression Design §2.2                    |
| `EventDefinition`                                                                                   | `eventType` enum (Windfall...Thief Gang), `category` (+/=/−), payload params (min/max gold, % HP mất...)                                                                                              | Progression Design §6.2                    |

> Nguyên tắc: **mọi con số GDD ghi `[TBD]` đều là field trên 1 trong các SO này** — không hardcode ở đâu khác kể cả tạm thời.

## 1.2. Domain Layer — `WW.Domain` (pure C#, không `UnityEngine`, không SO type)

|Class / Struct|Vai trò|Nguồn GDD|
|---|---|---|
|`MainAttributes` (struct)|`POT, SPI, WIS, VIT`|Game Overview §2.1.2|
|`ScalingWeights`, `SpellRuntimeData`, `EquipmentRuntimeData`, `RuneRuntimeData`, `EnemyRuntimeData`, `EffectRuntimeData`, `ComboRuleRuntimeData`, `NodeRuntimeData`, `EventRuntimeData`|Bản sao runtime của từng SO (do `ToRuntimeData()` sinh ra)|—|
|`PotencyCalculator` (static)|`spell_potency = Σ(weight_attr × attr_value)`|Combat Design §2.2.1|
|`ResistanceCalculator` (static)|`res_base = 10×(VIT/10)^p`; `damage_reduction = R/(R+90)`|Combat Design §2.2.2|
|`HPCalculator` (static)|`max_hp = HP_CAP × VIT/(VIT+HP_HALF)`|Combat Design §3.1|
|`MPCalculator` (static)|`max_mp = MP_COEFF × SPI`; `turn_mp_recovery = BASE + 1.0×SPI`|Combat Design §2.2.3, §3.3|
|`ElementType` (enum: Water, Fire, Ice, Lightning) + `Beats(a,b)`|Nước>Lửa>Băng>Sét>Nước|Game Overview §2.2.2|
|`ICombatant`, `PlayerCombatant`, `EnemyCombatant`|Adapter chung cho Player (tính từ MainAttributes) và Enemy (đọc thẳng từ định nghĩa)|Combat Design §4|
|`ActiveEffect`|Effect đang active trên 1 combatant, có `duration`, `isSelfStackable`|Combat Design §6|
|`ArmorStack` + `DamageApplier`|`value`, `duration`, `hurt_order`; resolve damage theo `hurt_order` cao nhất trước|Combat Design §3.2|
|`EquipmentLoadout` (4 slot), `RuneLoadout` (4 socket)|Trạng thái trang bị/rune hiện tại của Player|Game Overview §2.1.7, §2.1.9|
|`IStartPhaseStep` + 7 implementation (`MpRecoveryStep`, `FrozenCheckStep`, `CrystalizeCheckStep`, `RegenStep`, `BurnStep`, `RemainingStatusStep`, `ComboCheckStep`)|Đúng thứ tự resolve Start Phase|Combat Design §5.1.1|
|`IEndPhaseStep` + 5 implementation (`EffectDurationTickStep`, `EffectExpiryStep`, `ArmorDurationTickStep`, `ArmorExpiryStep`, `SpellCooldownTickStep`)|Đúng thứ tự resolve End Phase|Combat Design §5.1.3|
|`StartPhaseRunner`, `EndPhaseRunner`|Chạy list step theo thứ tự cố định, thêm step mới không sửa step cũ|Combat Design §5.1|
|`EffectResolver`|Áp dụng element interaction rule (khắc chế / neutralize cùng nguyên tố)|Game Overview §2.2.2|
|`ComboEvaluator`|Check + trigger Overdrive/Crystalize/Detonates/Frozen từ `ComboRuleRuntimeData[]`|Combat Design §6|
|`IEnemyBehaviorStrategy` (interface, **placeholder**) + `RandomBehaviorStrategy` (impl tạm)|Enemy chọn spell nào để cast|Combat Design §4 — _chờ Enemy Design GDD_|
|`DomainEventBus` + payload struct (`DamageDealtPayload`, `EffectAppliedPayload`...)|Điểm nối Domain → Controller — xem mục 3.1|— (mới thêm)|
|`CombatState`|Player + enemy list + turn hiện tại + phase hiện tại|Combat Design §5|
|`TurnController`|`CastSpell()`, `AdvanceTurn()` — nơi Controller gọi vào|Game Overview §2.3.2|
|_(Progression, thứ yếu cho MVP)_: `MapGraph`, `MapGraphGenerator`, `SpellSlotUnlockCalculator` (dùng `WisdomSlotConfig`), `ShopInventoryGenerator`, `ShopService`, `RewardRoller`, `IEventEffect` + 10 impl, `EventResolver`|Sinh map, shop, reward, event|Progression Design toàn bộ|

## 1.3. Event Layer — `WW.Events` (SO Event Channel, concrete class — không cần generic)

|Channel|Payload|Bắn khi|
|---|---|---|
|`DamageEventChannel`|target, element, amount|`TurnController` gây damage|
|`EffectChangedEventChannel`|target, effectType, `changeType` (Applied/Expired/Neutralized)|Effect apply/hết hạn/bị giải|
|`TurnPhaseChangedEventChannel`|`TurnPhase` mới|Start/Action/End Phase đổi|
|`CombatEndedEventChannel`|`CombatResult` (Win/Lose)|Win/Lose condition true|
|`SpellCastEventChannel`|caster, spellId|Ngay khi cast (tách khỏi Damage vì spell buff không gây damage)|
|_(sau, cho Progression)_: `NodeEnteredEventChannel`, `ShopOpenedEventChannel`, `GoldChangedEventChannel`|—|—|

> Quyết định luôn: **dùng concrete class, không dùng `GameEventSO<T>` generic** (mục 10 bản gốc để mở — mình chốt theo đúng đề xuất "bắt đầu bằng concrete" trong chính bản gốc, vì generic khó debug trên Inspector và team có non-tech).

## 1.4. Controller Layer — `WW.Controllers` _(asmdef mới thêm)_

|Class|Vai trò|
|---|---|
|`CombatController` (MonoBehaviour)|Sở hữu `CombatState`, subscribe `DomainEventBus`, forward sang SO Event Channel, expose method public cho input (`OnSpellSelected`...)|
|`MapController` / `RunController` _(sau)_|Điều phối di chuyển giữa Node, sở hữu `MapGraph`|
|`ShopController` _(sau)_|Mở/đóng Shop, gọi `ShopService`|

## 1.5. Presentation Layer — `WW.Presentation`

|View|Đọc từ|Ghi (input)|
|---|---|---|
|`HealthBarView`, `MpBarView`, `ArmorStackView`|`DamageEventChannel` (delta) + query 1 lần lúc init|—|
|`SpellButtonView`, `SpellSlotView`|`SpellDefinition` (Data, để hiển thị icon/cost)|gọi `CombatController.OnSpellSelected()`|
|`BuffIconView` / `DebuffIconView`|`EffectChangedEventChannel`|—|
|`TurnBannerView`|`TurnPhaseChangedEventChannel`|—|
|`CombatResultView`|`CombatEndedEventChannel`|nút "Tiếp tục" → gọi Controller|
|_(sau)_ `NodeMapView` (render fog of war), `ShopItemView`, `EventPopupView`|—|—|

---

# 2. Assembly Reference Graph chính xác

Sơ đồ ASCII ở bản gốc (Presentation → Event → Domain → Data) là **luồng dữ liệu khái niệm**, không phải sơ đồ reference asmdef thật. Reference graph thật (5 asmdef):

|Assembly|Được reference tới|**Không được** reference|
|---|---|---|
|`WW.Data`|_(không reference project nào khác, chỉ UnityEngine)_|WW.Domain, WW.Events, WW.Controllers, WW.Presentation|
|`WW.Domain`|_(không reference gì trong project)_|**UnityEngine** (dùng `System.Math`), WW.Data (chỉ nhận runtime struct, không nhận SO type), WW.Events, WW.Controllers, WW.Presentation|
|`WW.Domain.Tests` (EditMode)|WW.Domain, NUnit|UnityEngine (không cần, vì Domain không cần)|
|`WW.Events`|WW.Domain (chỉ để dùng type cho payload)|WW.Data, WW.Controllers, WW.Presentation|
|`WW.Controllers`|WW.Data, WW.Domain, WW.Events|WW.Presentation (Controller không được biết View cụ thể nào tồn tại)|
|`WW.Presentation`|WW.Events, WW.Controllers, WW.Data _(chỉ để đọc field hiển thị như icon/tên — xem mục 3.3)_|_(xem mục 3.3 về WW.Domain)_|

**Vì sao thêm `WW.Controllers`:** nếu Controller code nằm chung asmdef với Presentation (như bản gốc ngầm định), Presentation sẽ _transitively_ có quyền gọi thẳng Data + Domain thông qua asmdef reference của Controller — đúng thứ mà cả hệ thống asmdef được dựng lên để chặn. Tách riêng thì Presentation chỉ còn đường hợp lệ duy nhất để mutate state là gọi qua `WW.Controllers`.

---

# 3. Quy tắc giao tiếp giữa các layer

Có **2 chiều giao tiếp khác nhau, không đối xứng** — đây là điểm bản gốc gộp chung dễ gây nhầm cho junior.

## 3.1. Output contract — Domain → Presentation (state đổi → hiển thị)

```
Domain (raise plain C# event trên DomainEventBus)
   → Controller (subscribe bus, forward sang SO Event Channel)
      → Presentation (subscribe SO Event Channel)
```

```csharp
// WW.Domain — KHÔNG reference UnityEngine, KHÔNG reference WW.Events
namespace WW.Domain.Events
{
    public readonly struct DamageDealtPayload
    {
        public readonly ICombatant Target;
        public readonly ElementType Element;
        public readonly float Amount;
        public DamageDealtPayload(ICombatant target, ElementType element, float amount)
        { Target = target; Element = element; Amount = amount; }
    }

    // Gom toàn bộ event Domain phát ra vào 1 chỗ — Controller chỉ subscribe 1 object duy nhất
    public class DomainEventBus
    {
        public event Action<DamageDealtPayload> DamageDealt;
        public event Action<EffectChangedPayload> EffectChanged;
        public event Action<TurnPhase> TurnPhaseChanged;
        public event Action<CombatResult> CombatEnded;

        // internal: chỉ gọi được từ trong WW.Domain (TurnController, EffectResolver...)
        internal void RaiseDamageDealt(DamageDealtPayload p) => DamageDealt?.Invoke(p);
        internal void RaiseEffectChanged(EffectChangedPayload p) => EffectChanged?.Invoke(p);
    }
}
```

```csharp
// WW.Controllers
public class CombatController : MonoBehaviour
{
    [SerializeField] private DamageEventChannel _damageChannel;
    [SerializeField] private EffectChangedEventChannel _effectChannel;

    private DomainEventBus _bus;
    private CombatState _state;

    private void StartCombat(EnemyDefinition[] enemies)
    {
        _bus = new DomainEventBus();
        _bus.DamageDealt += p => _damageChannel.Raise(new DamageEventPayload(p.Target, p.Element, p.Amount));
        _bus.EffectChanged += p => _effectChannel.Raise(p);
        _state = new CombatState(_bus, /* runtime data từ Data Layer */);
    }
}
```

> **Quy tắc cứng:** Domain **không bao giờ** `new DamageEventChannel()` hay giữ reference tới bất kỳ SO nào. Nếu 1 class trong Domain cần "bắn UI" — nó chỉ được gọi `_eventBus.RaiseXxx(...)`, chấm hết.

## 3.2. Input contract — Presentation → Controller (người chơi thao tác → mutate state)

**Không đi qua Event Channel.** Đây là lời gọi hàm trực tiếp, vì Controller chính là entry point cho input, không phải listener.

```csharp
// WW.Presentation
public class SpellButtonView : MonoBehaviour
{
    [SerializeField] private SpellDefinition _spell; // đọc data để hiển thị icon/cost
    [SerializeField] private CombatController _controller;

    public void OnClick() => _controller.OnSpellSelected(_spell); // gọi thẳng, KHÔNG qua event channel
}
```

```csharp
// WW.Controllers
public void OnSpellSelected(SpellDefinition spell)
    => _state.TurnController.CastSpell(_state.Player, _state.CurrentTarget, spell.ToRuntimeData());
```

## 3.3. Read contract — Presentation đọc state hiện tại (không phải delta)

Event Channel chỉ mang **delta** ("vừa mất 15 HP"), nhưng View cần giá trị **tuyệt đối** lúc khởi tạo (VD: `HealthBarView` cần biết `currentHP/maxHP` ngay khi mở scene, trước khi có event nào bắn ra). 2 mức nghiêm ngặt:

|Mức|Cách làm|Khi nên dùng|
|---|---|---|
|**Lỏng** (nhanh hơn, ít boilerplate)|Presentation reference thẳng `WW.Domain`, đọc property (`combatant.CurrentHP`) khi cần. Quy ước bắt buộc: method mutate trong Domain luôn đặt tên là **verb hành động** (`Apply`, `Tick`, `Cast`, `Resolve`), method đọc luôn là **property/`Get*`** — review bằng mắt khi PR|Progression layer, phần ít đổi, ít người đụng|
|**Chặt** (compiler enforce thật)|Presentation **không** reference `WW.Domain`. Controller expose sẵn property/DTO read-only (VD `public float CurrentHpRatio => _state.Player.CurrentHP / _state.Player.MaxHP;`)|Combat layer — nơi 2-4 người cùng đụng code, GDD note rõ "chắc chắn sẽ mở rộng"|

**Khuyến nghị:** dùng mức **Chặt** cho Combat Controller (đúng tinh thần asmdef ban đầu của chính bạn — "biến convention thành lỗi compile"), mức **Lỏng** chấp nhận được ở Progression vì đó là phần rút gọn, ít rủi ro hơn cho MVP. Đây là điểm có đánh đổi thật (thêm vài property wrapper ở Controller) — không phải đúng/sai tuyệt đối, bạn có thể chọn Lỏng cho cả hai nếu ưu tiên tốc độ hơn.

## 3.4. Bảng tổng hợp quyền gọi

|Từ \ Đến|Data|Domain|Events|Controllers|Presentation|
|---|---|---|---|---|---|
|**Data**|—|❌|❌|❌|❌|
|**Domain**|chỉ runtime struct (`ToRuntimeData()` output)|—|❌ (raise qua `DomainEventBus` thuần C#)|❌|❌|
|**Events**|❌|dùng type cho payload|—|❌|❌|
|**Controllers**|✅ đọc SO|✅ gọi mutate + subscribe `DomainEventBus`|✅ `.Raise()`|—|❌ (không được biết View cụ thể)|
|**Presentation**|✅ đọc (icon, tên, cost để hiển thị)|mức Lỏng: ✅ đọc / mức Chặt: ❌|✅ subscribe|✅ gọi method input|—|

---

# 4. Thứ tự code

Nguyên tắc chọn thứ tự: **logic thuần trước, wiring Unity sau, visual cuối cùng** — vì logic thuần test bằng EditMode (mili giây, không cần scene), còn Play Mode chỉ nên dùng để verify wiring/cảm giác chơi, không phải để bắt lỗi công thức.

|Phase|Nội dung|Layer|Test|Điều kiện qua Phase|
|---|---|---|---|---|
|**0**|5 asmdef (Data, Domain, Domain.Tests, Events, Controllers, Presentation) + folder structure + git|—|Cố ý `using UnityEngine;` trong `WW.Domain` → phải thấy lỗi compile|Lỗi compile xuất hiện đúng như kỳ vọng|
|**1**|`CombatBalanceConfig`, `ProgressionBalanceConfig`, `WisdomSlotConfig` + `SpellDefinition`/`EquipmentDefinition`/`RuneDefinition`/`EnemyDefinition`/`EffectDefinition` (field đầy đủ, `ToRuntimeData()` stub)|Data|Tạo được asset trên Inspector, không lỗi|Designer bắt đầu điền được data thật song song với Phase 2+|
|**2**|`PotencyCalculator`, `ResistanceCalculator`, `HPCalculator`, `MPCalculator`, `ElementType.Beats()`|Domain (formula thuần)|EditMode: so khớp đúng bảng tham khảo GDD (VD VIT=50→res_base=75.8; VIT=90→max_hp=500)|Mọi bảng tham khảo trong Combat Design pass test|
|**3**|`MainAttributes`, `ICombatant`, `PlayerCombatant`, `EnemyCombatant`, `ActiveEffect`, `ArmorStack`+`DamageApplier`, `IEnemyBehaviorStrategy` (placeholder)|Domain (entity)|EditMode: damage tràn đúng thứ tự `hurt_order` (ví dụ §3.2 GDD Combat Design)|Ví dụ Armor 2 stack trong GDD cho kết quả đúng|
|**4**|`IStartPhaseStep`×7, `IEndPhaseStep`×5, `StartPhaseRunner`, `EndPhaseRunner`|Domain (turn engine)|EditMode: log thứ tự step chạy, so với bảng GDD §5.1.1/§5.1.3|Thứ tự resolve khớp 100% bảng GDD|
|**5**|`EffectResolver` (element interaction), `ComboEvaluator` (đọc `ComboRuleRuntimeData[]`)|Domain (effect/combo — điểm nóng nhất)|EditMode: case Nước giải Lửa; case Enrage+Energized→Overdrive kích hoạt đúng lượt sau|Toàn bộ bảng §2.2.2 (Game Overview) + §6 (Combat Design) có test case|
|**6**|`DomainEventBus`, `CombatState`, `TurnController.CastSpell()`|Domain (glue nội bộ)|EditMode integration: cast 1 spell lửa lên bản thân → subscribe `DomainEventBus` ngay trong test, verify payload đúng|Luồng "cast spell" ví dụ §8 bản gốc chạy được **hoàn toàn không cần Unity**|
|**7**|5-6 SO Event Channel cụ thể cho Arc 1|Events|Compile only|—|
|**8**|`CombatController` (StartCombat, OnSpellSelected, forward bus→channel)|Controllers|**Play Mode đầu tiên**: scene trống, hardcode 1 enemy, gọi `OnSpellSelected` qua Context Menu tạm, xem `Debug.Log` ở listener|Event Channel nhận đúng payload trong Play Mode|
|**9**|`HealthBarView`, `SpellButtonView`, `BuffIconView`, `TurnBannerView`, `CombatResultView`|Presentation|Play Mode: chơi thử combat 1vs1 bằng UI thật|Thắng/thua 1 trận qua UI, không qua code debug|
|**10**|Điền data thật Arc 1 (10-12 spell theo scope MVP, equipment/rune/enemy tương ứng)|Data (content)|Playtest thủ công|Chơi hết Arc 1 combat loop không crash|
|**11** _(sau, rút gọn cho MVP)_|`NodeDefinition`, `ArcMapConfig` (map cố định nhỏ, chưa cần full generation algorithm), `ShopController`, `RewardRoller`, `EventResolver` (10 event)|Data+Domain+Controllers+Presentation|Play Mode|Đi được từ đầu Arc 1 đến Boss cuối|

> Phase 11 cố tình để cuối và "rút gọn" — đúng scope MVP đã ghi ở đầu file gốc ("Progression rút gọn"), và vì thuật toán sinh layout map tự GDD Progression Design đã note là "📄 Tech Design — chưa có" ở mục 7.

---

# 5. Checklist trước khi bắt đầu Phase 0

- [ ] Chốt dùng `System.Math` (không `Mathf`) trong toàn bộ `WW.Domain` — ghi thành convention mục 7 file gốc.
- [ ] 5 asmdef (không phải 4) — thêm `WW.Controllers`.
- [ ] Chọn giá trị **tạm** cho `BASE_MP_RECOVERY` và exponent `p` (đang `[TBD]` trong GDD) — không cần đúng số cuối, chỉ cần có số để Phase 2 viết test được. Balancing thật làm sau, không block code.
- [ ] Quyết định mức Lỏng/Chặt cho Presentation↔Domain (mục 3.3) — áp dụng nhất quán, đừng để mỗi View một kiểu.
- [ ] Tạo `WW.Domain.Tests.asmdef` (EditMode, reference `WW.Domain` + NUnit) song song với `WW.Domain` ngay từ Phase 2, đừng để dồn viết test sau cùng.

---

# 6. Folder Structure chi tiết

Bản gốc (§5) để asset thật (`BalanceConfig.asset`) nằm chung folder với định nghĩa class (`SpellDefinition.cs`) — hai thứ khác bản chất: **code** ổn định, ít file, chỉ dev sửa; **content** phình nhanh theo Arc, designer/non-tech sửa suốt. Tách riêng `Scripts/` (theo asmdef) và `Content/` (asset instance) để non-tech chỉ cần biết `Content/`, không bao giờ phải mở `Scripts/`.

Trong `Scripts/`, chia theo **feature trước, kỹ thuật sau** (không có `Interfaces/` nằm ngang hàng top-level) — sửa 1 effect mới chỉ mở đúng 1 folder.

```
Assets/
  _Project/
    Scripts/
      WW.Data/                          [WW.Data.asmdef]
        BalanceConfig/
          CombatBalanceConfig.cs
          ProgressionBalanceConfig.cs
          WisdomSlotConfig.cs
        Spells/SpellDefinition.cs
        Equipment/EquipmentDefinition.cs
        Runes/RuneDefinition.cs
        Effects/
          EffectDefinition.cs
          ComboRuleDatabase.cs
        Enemies/EnemyDefinition.cs
        Progression/                    # sau
          NodeDefinition.cs
          ArcMapConfig.cs
          EventDefinition.cs

      WW.Domain/                        [WW.Domain.asmdef]
        Combat/
          Formulas/     PotencyCalculator.cs, ResistanceCalculator.cs, HPCalculator.cs, MPCalculator.cs
          Elements/     ElementType.cs
          Combatants/   ICombatant.cs, PlayerCombatant.cs, EnemyCombatant.cs, EquipmentLoadout.cs, RuneLoadout.cs
          Effects/      ActiveEffect.cs, ArmorStack.cs, DamageApplier.cs, EffectResolver.cs, ComboEvaluator.cs
          TurnEngine/
            StartSteps/   7 step class (MpRecoveryStep, FrozenCheckStep, CrystalizeCheckStep, RegenStep,
                          BurnStep, RemainingStatusStep, ComboCheckStep)
            EndSteps/     5 step class (EffectDurationTickStep, EffectExpiryStep, ArmorDurationTickStep,
                          ArmorExpiryStep, SpellCooldownTickStep)
            StartPhaseRunner.cs, EndPhaseRunner.cs
          Enemies/      IEnemyBehaviorStrategy.cs, RandomBehaviorStrategy.cs
          RuntimeData/  SpellRuntimeData.cs, EquipmentRuntimeData.cs, RuneRuntimeData.cs, EnemyRuntimeData.cs,
                        EffectRuntimeData.cs, ComboRuleRuntimeData.cs
          Events/       DomainEventBus.cs, Payloads/
          CombatState.cs, TurnController.cs
        Progression/                    # sau — MapGraph.cs, MapGraphGenerator.cs, SpellSlotUnlockCalculator.cs,
                                         # ShopInventoryGenerator.cs, ShopService.cs, RewardRoller.cs,
                                         # IEventEffect.cs + 10 impl, EventResolver.cs

      WW.Domain.Tests/                  [WW.Domain.Tests.asmdef, EditMode]
        Combat/                         # mirror y hệt cấu trúc WW.Domain/Combat — mở 1 class biết ngay test ở đâu

      WW.Events/                        [WW.Events.asmdef]
        Combat/       DamageEventChannel.cs, EffectChangedEventChannel.cs, TurnPhaseChangedEventChannel.cs,
                      CombatEndedEventChannel.cs, SpellCastEventChannel.cs
        Progression/                    # sau — NodeEnteredEventChannel.cs, ShopOpenedEventChannel.cs,
                                         # GoldChangedEventChannel.cs

      WW.Controllers/                   [WW.Controllers.asmdef]
        CombatController.cs
        MapController.cs                # sau
        ShopController.cs               # sau

      WW.Presentation/                  [WW.Presentation.asmdef]
        Combat/       HealthBarView.cs, MpBarView.cs, ArmorStackView.cs, SpellButtonView.cs, SpellSlotView.cs,
                      BuffIconView.cs, DebuffIconView.cs, TurnBannerView.cs, CombatResultView.cs
        Map/                            # sau — NodeMapView.cs
        Shop/                           # sau — ShopItemView.cs
        Event/                          # sau — EventPopupView.cs

    Content/                            # asset instance thật — designer làm ở đây, KHÔNG có code, KHÔNG có asmdef
      BalanceConfig/
        CombatBalanceConfig.asset
        ProgressionBalanceConfig.asset
        WisdomSlotConfig.asset
      Combos/ComboRuleDatabase.asset
      Spells/
        Arc1/  Arc2/  Arc3/
      Equipment/
        Arc1/  Arc2/  Arc3/
      Runes/
        Arc1/  Arc2/  Arc3/
      Enemies/
        Arc1/  Arc2/  Arc3/
      Nodes/
      Events/

    Prefabs/
      Combat/  HealthBar.prefab, SpellButton.prefab, BuffIcon.prefab
      HUD/
      Map/

    Scenes/
      Combat_Sandbox.unity              # scene trắng dùng cho Phase 8 (checkpoint Play Mode đầu tiên)
      MainGame.unity

    Art/
    Audio/
```

**Quy tắc đi kèm:**

- Namespace khớp path folder (VD `WW.Domain.Combat.Effects` ↔ `Scripts/WW.Domain/Combat/Effects/`) — dễ tìm trong IDE, đúng convention "1 class = 1 file" của file gốc.
- `Content/` chia theo Arc bên trong từng loại item (không phải theo loại item bên trong từng Arc) — vì cân bằng 1 loại item (VD toàn bộ Spell) xuyên Arc là việc làm thường xuyên hơn so với xem toàn bộ nội dung 1 Arc cùng lúc.
- `Combat_Sandbox.unity` nên tồn tại xuyên suốt dự án (không xoá sau Phase 8) — dùng để test riêng 1 combat encounter mà không cần chạy full run từ đầu map, tiết kiệm thời gian test rất nhiều khi balance sau này.

---

# 7. Đặc tả chi tiết từng Class

Format mỗi class: **Vai trò** (làm gì) → **Giới hạn** (không được làm gì) → **Giao tiếp** (nói chuyện với ai, bằng cách nào) → **Requirement** (nguồn GDD) → **Acceptance Criteria** (điều kiện test được, checklist).

Domain layer được đặc tả đầy đủ nhất vì đây là nơi rủi ro cao nhất. Data/Events/Controllers/Presentation đặc tả gọn hơn vì phần lớn là structural contract, ít business logic.

## 7.1. Domain — Formulas (pure static function)

|Class|Requirement (GDD)|Acceptance Criteria|
|---|---|---|
|`PotencyCalculator`|`spell_potency = Σ(weight_attr × attr_value)`, default weight `{POT:1.0}` nếu Spell không khai báo (Combat Design §2.2.1)|weights={POT:1}, POT=10 → 10 (khớp hành vi cũ) · weights={SPI:0.7,WIS:0.3}, SPI=20,WIS=10 → 17 · weight=0 cho 1 attr → attr đó không góp phần dù giá trị lớn · không giữ state giữa 2 lần gọi|
|`ResistanceCalculator`|`res_base=10×(VIT/10)^p`; `damage_reduction=R/(R+90)`; `actual_damage=incoming×90/(R+90)` (Combat Design §2.2.2)|VIT=10, mọi p → res_base=10.0 (mốc cố định) · VIT=50,p=1.3 → res_base≈75.8 · R=90 → giảm 50% · R→∞ → tiệm cận 100%, không bao giờ chạm 100%|
|`HPCalculator`|`max_hp=HP_CAP×VIT/(VIT+HP_HALF)` (Combat Design §3.1)|VIT=10→100 · VIT=90→500 (khớp bảng GDD)|
|`MPCalculator`|`max_mp=MP_COEFF×SPI`; `turn_mp_recovery=BASE_MP_RECOVERY+1.0×SPI` (Combat Design §2.2.3, §3.3)|SPI=10, MP_COEFF=10 → max_mp=100|
|`ElementType.Beats(a,b)`|Nước>Lửa>Băng>Sét>Nước, cyclic (Game Overview §2.2.2)|**Xem bug đã sửa ở trên** — `Beats(Water,Fire)=true`, `Beats(Fire,Water)=false`, `Beats(Lightning,Water)=true`, và ngược lại của cả 4 cặp phải false|

```csharp
// Bản sửa đúng
public enum ElementType { Water, Fire, Ice, Lightning }
public static bool Beats(ElementType a, ElementType b)
    => ((int)b - (int)a + 4) % 4 == 1; // a khắc b khi b đứng NGAY SAU a trong vòng lặp
```

## 7.2. Domain — Combatants

**`ICombatant`**

- **Vai trò:** abstraction chung để mọi code Domain khác (TurnController, EffectResolver, DamageApplier...) xử lý Player/Enemy như nhau.
- **Giới hạn:** không expose field raw (không có `set` public trên `CurrentHP`) — mọi thay đổi phải qua method mutate rõ tên (`TakeDamage`, `ApplyEffect`), tránh code ngoài tự ý phá invariant.
- **Giao tiếp:** implement bởi `PlayerCombatant`/`EnemyCombatant`; dùng bởi hầu hết Domain class khác qua interface, không bao giờ cast ngược về class cụ thể trừ khi thực sự cần (VD hiển thị UI khác nhau Player/Enemy — nên tránh, đẩy khác biệt đó vào Presentation).
- **Requirement:** expose `MaxHP`, `CurrentHP`, `GetPotency(ElementType)`, `GetResistance(ElementType)`, `ActiveEffects` (readonly), `ArmorStacks` (readonly), `TakeDamage(amount, element)`, `ApplyEffect(effect)`.
- **AC:** Domain code gọi `combatant.TakeDamage()` không cần biết đang gọi lên Player hay Enemy — cùng 1 dòng code chạy đúng cho cả 2.

**`PlayerCombatant`**

- **Giới hạn:** không tự đọc `SpellDefinition` (SO) — chỉ nhận `SpellRuntimeData` qua tham số method, giữ đúng nguyên tắc Domain không biết Data.
- **Requirement:** `MaxHP = HPCalculator(VIT sau equipment/rune)`; Resistance = `ResistanceCalculator(VIT) + resistance bonus từ Equipment/Rune` (Combat Design §2.2.2 — 2 nguồn cộng riêng biệt).
- **AC:** VIT gốc 10 + Equipment cộng 40 → tính res_base theo VIT=50, KHÔNG theo VIT=10 · nếu Equipment cộng thêm `fire_res` riêng (không qua VIT) → `fire_res_final = res_base + fire_res_bonus`.

**`EnemyCombatant`**

- **Giới hạn:** **không** dùng `HPCalculator`/`ResistanceCalculator` — đọc thẳng giá trị từ `EnemyRuntimeData` (Combat Design §4: "Enemy không dùng hệ Main Attribute").
- **AC:** `EnemyCombatant.MaxHP == EnemyRuntimeData.maxHp` chính xác, không qua transform nào.

**`EquipmentLoadout` / `RuneLoadout`**

- **Requirement:** `RuneLoadout` luôn có đúng 4 socket ngay từ đầu run, không cần unlock (Game Overview §2.1.9); Embed/Purge miễn phí, không giới hạn số lần, chỉ được gọi ngoài combat (Game Overview §2.1.8).
- **Giao tiếp:** `CombatController`/`RunController` (ngoài combat) gọi `RuneLoadout.Embed()/Purge()`; **không** gọi được từ trong `CombatState` đang chạy trận (guard clause chặn nếu `CombatState.IsActive == true`).
- **AC:** gọi `Embed()` khi đang trong combat → phải bị chặn (assert/exception), không âm thầm no-op.

## 7.3. Domain — Effects & Combo

**`ActiveEffect`**

- **Giới hạn:** không tự "tick" bản thân — không có `Update()`/coroutine nội bộ. Duration chỉ giảm khi bị gọi từ ngoài (`EffectDurationTickStep`), đảm bảo effect không bao giờ resolve sai thời điểm trong Turn Structure.
- **Requirement:** non-self-stackable effect apply lại chỉ refresh duration, không tạo instance thứ 2 (Combat Design §6, mọi effect trừ Regen).
- **AC:** Apply Enrage lần 2 lên combatant đã có Enrage → vẫn 1 instance · Apply Regen 2 lần → 2 instance riêng · Duration=∞ (Enrage, Burn...) không tự giảm ở End Phase trừ khi bị Neutralize bởi `EffectResolver`.

**`ArmorStack` + `DamageApplier`**

- **Requirement:** HP luôn `hurt_order=0`; Armor mới = `current_max_hurt_order+1`; damage trừ vào `hurt_order` cao nhất trước, tràn xuống khi `value=0` (Combat Design §3.2).
- **Giới hạn:** `DamageApplier` **không** tự trừ Resistance — Resistance phải áp dụng trước khi damage tới `DamageApplier` (thứ tự bắt buộc: raw damage → `ResistanceCalculator` → `DamageApplier`).
- **AC:** dựng lại đúng ví dụ GDD — Stack A (`hurt_order=1, value=30, dur=2`), Stack B (`hurt_order=2, value=20, dur=1`), nhận 35 dmg → B về 0 bị xoá, 15 tràn sang A → A còn 15, HP không đổi.

**`EffectResolver`**

- **Vai trò:** xử lý element interaction khi apply effect mới lên combatant đã có effect khác active.
- **Requirement** (Game Overview §2.2.2): B active + apply A mà B khắc A → A bị giải, B giữ nguyên · B active + apply A mà A khắc B → B bị giải, A apply · cùng nguyên tố → cả 2 neutralize.
- **Giao tiếp:** gọi `ElementType.Beats()`, đọc/ghi `ActiveEffect` list trên `ICombatant`; được gọi bởi `TurnController.CastSpell()` ngay sau khi tính potency, trước `ComboEvaluator`.
- **AC:** Target có Chilled(Ice), bị Burn(Fire) — Fire khắc Ice → Chilled giải, Burn apply · Target có Burn(Fire), bị Drenched(Water) — Water khắc Fire → Burn giải, Drenched apply · Target có Burn(Fire), bị thêm 1 phép Lửa khác lên chính nó → cả 2 neutralize.

**`ComboEvaluator`**

- **Requirement:** đọc `ComboRuleRuntimeData[]` (không hardcode), check Overdrive/Crystalize/Detonates/Frozen sau khi có đủ 2 effect điều kiện (any order) (Combat Design §6).
- **Giao tiếp:** được gọi ở **2 điểm** — cuối `StartPhaseRunner` (`ComboCheckStep`) VÀ ngay sau mỗi lần `EffectResolver` apply effect mới trong Action Phase (vì combo có thể trigger giữa lượt, không chỉ đầu lượt).
- **AC:** Có Enrage, apply thêm Energized (bất kỳ thứ tự) → Overdrive kích hoạt cho lượt **kế tiếp**, tự mất sau đúng 1 lượt · Detonates trigger **tức thời** (Instant) ngay khi đủ Burn+Dazed, không đợi lượt sau, không đợi End Phase.

## 7.4. Domain — Turn Engine

**`StartPhaseRunner`** — chạy đúng 7 step theo thứ tự cố định (Combat Design §5.1.1). Thêm step mới = thêm vào mảng, không sửa step cũ.

|#|Step|Logic|AC quan trọng|
|---|---|---|---|
|1|`MpRecoveryStep`|`current_mp=min(current_mp+turn_mp_recovery,max_mp)`|MP không vượt max dù recovery lớn|
|2|`FrozenCheckStep`|Nếu Frozen active → set `SkipAction=true`, remove Frozen ngay (không đợi End Phase)|Frozen chỉ hiệu lực đúng 1 lần rồi mất, dù effect duration ghi "next turn only"|
|3|`CrystalizeCheckStep`|Nếu Crystalize active → bật `DamageDebuffImmuneFlag` cho lượt này|Flag bật **trước** Burn (step 5) · Crystalize mới apply trong chính lượt này **không** có hiệu lực ngay — chỉ bảo vệ từ lượt sau|
|4|`RegenStep`|Hồi `regen_hp_percent × max_hp`|Chạy **trước** Burn — test case: HP thấp, Regen đủ để sống sót Burn ngay sau|
|5|`BurnStep`|Gây `burn_base_dmg + burn_hp_percent×max_hp` fire damage|**Skip nếu** `DamageDebuffImmuneFlag` đang bật (từ step 3)|
|6|`RemainingStatusStep`|Enrage/Drenched/Chilled/Dazed/Fortified/Energized — hiện tại đa số là modifier đọc liên tục (all_potencies/all_resistances), không cần resolve riêng ở Start; step này chủ yếu để dành chỗ cho effect mới sau này|Có thể là no-op cho Arc 1, nhưng **phải tồn tại như 1 step riêng** (không xoá) để effect mới không phải sửa lại thứ tự|
|7|`ComboCheckStep`|Gọi `ComboEvaluator` sau khi mọi effect khác đã resolve|Chạy **sau cùng**, không trước bước 6|

**`EndPhaseRunner`** — 5 step (Combat Design §5.1.3):

|#|Step|Logic|AC quan trọng|
|---|---|---|---|
|1|`EffectDurationTickStep`|Giảm duration mọi effect|**Chỉ tick effect của combatant đang kết thúc lượt** — effect của đối phương không bị đụng tới|
|2|`EffectExpiryStep`|Xoá effect về 0 duration|—|
|3|`ArmorDurationTickStep`|Giảm duration Armor stack|GDD ghi rõ: "Duration giảm vào End Phase **của lượt bên sở hữu Armor**" — Player's Armor không giảm khi Enemy kết thúc lượt|
|4|`ArmorExpiryStep`|Xoá Armor stack về 0 duration, **bất kể còn value**|Armor còn 20 value nhưng duration=0 → vẫn bị xoá, không giữ lại|
|5|`SpellCooldownTickStep`|Giảm cooldown mọi spell đang cd|—|

## 7.5. Domain — Enemy Behavior _(placeholder, chờ Enemy Design GDD)_

**`IEnemyBehaviorStrategy`**

- **Giới hạn:** `EnemyCombatant`/`TurnController` **không được** if/else theo `EnemyType` để chọn spell — mọi lựa chọn đi qua `SelectNextSpell()`.
- **Requirement tạm** (`RandomBehaviorStrategy`): chọn ngẫu nhiên đều trong spell gắn sẵn, loại trừ spell đang cooldown.
- **AC:** Enemy có 2 spell, 1 spell đang cd → chỉ chọn spell còn lại · tất cả spell đều cd → skip lượt (pass), không throw exception.

## 7.6. Domain — Glue

**`DomainEventBus`**

- **AC:** `Raise` không throw nếu chưa có subscriber (`?.Invoke`) · mỗi trận combat mới phải tạo **instance mới** (không reuse bus của trận trước) để tránh leak subscriber cũ.

**`CombatState` / `TurnController`**

- **Vai trò:** `CombatState` giữ state 1 trận (player, enemy list, turn owner, phase hiện tại). `TurnController.CastSpell()`/`AdvanceTurn()` là entry point orchestrate Formulas + Combatants + Effects + TurnEngine.
- **Giới hạn:** không quyết định UI feedback — chỉ raise `DomainEventBus`.
- **Requirement:** check Win/Lose **ngay sau mỗi lần damage**, không đợi hết lượt (Game Overview §2.3.2).
- **AC:** Enemy cuối cùng về 0 HP giữa Action Phase → `CombatEnded(Win)` raise ngay lập tức · Player về 0 HP do Burn ở Start Phase → `CombatEnded(Lose)` raise ngay tại Start Phase, các step Start Phase còn lại **không chạy tiếp**.

## 7.7. Data Layer

**Quy tắc chung cho mọi SO:** `ToRuntimeData()` chỉ copy field, tuyệt đối không chứa logic tính toán (logic thuộc Domain). AC chung: đổi 1 field trên Inspector → lần convert kế tiếp nhận giá trị mới ngay, không cache stale.

|Class|Requirement riêng|AC riêng|
|---|---|---|
|`SpellDefinition`|`minWisdomToImprint` độc lập với `WisdomSlotConfig` (2 khái niệm khác nhau — Game Overview §2.1.2/§2.1.3)|WIS đủ mở slot nhưng chưa đủ `minWisdomToImprint` của spell → vẫn không Imprint được|
|`EnemyDefinition`|Số spell gắn sẵn khớp `enemyType` (Minion 1-2, Elite 2-3, Boss 3-4 — Combat Design §4)|Validate ở Editor: tạo Enemy Minion với 3 spell → cảnh báo (không bắt buộc block, chỉ warn vì designer có thể có lý do)|
|`EffectDefinition`|`resolveTime` phải là 1 trong Start/End/Instant, khớp đúng bảng §6 Combat Design|Burn→Start; Frozen→Instant lúc combo trigger nhưng hiệu lực check ở Start (FrozenCheckStep)|
|`ComboRuleDatabase`|Chỉ chứa 4 rule hiện có (Overdrive/Crystalize/Detonates/Frozen), thêm rule mới = thêm entry, không sửa `ComboEvaluator`|—|

## 7.8. Event Layer

- **Requirement chung:** mọi Channel là concrete SO class (không generic), 1 asset duy nhất cho mỗi channel.
- **Giới hạn/gotcha quan trọng nhất:** `CombatController` (raise) và mọi View (listen) **phải reference cùng 1 asset instance** — nếu vô tình tạo 2 asset `DamageEventChannel` khác nhau (VD 1 cái trong `Content/`, 1 cái test), event sẽ bắn nhưng không tới listener, **không có lỗi compile, không có exception** — bug im lặng, khó debug nhất trong cả kiến trúc này.
- **AC:** setup 1 scene test, cố tình gán 2 asset Channel khác nhau cho Controller và View → xác nhận View không nhận được event (để hiểu rõ triệu chứng trước khi nó xảy ra thật).

## 7.9. Controllers — `CombatController`

- **Vai trò:** MonoBehaviour duy nhất biết cả Domain lẫn Events; sở hữu vòng đời `CombatState`; entry point cho input từ Presentation.
- **Giới hạn:** không chứa formula/rule nào (mọi quyết định đẩy xuống Domain) · không reference bất kỳ type nào trong `WW.Presentation` (theo mục 2, `WW.Controllers` không được reference `WW.Presentation`) · nếu phình quá 300-400 dòng hoặc nhiều nhánh if → dấu hiệu cần tách thêm Domain class, không thêm logic vào Controller.
- **Giao tiếp:** subscribe `DomainEventBus` (5-6 event) → forward `.Raise()` sang Event Channel tương ứng · expose method public cho Presentation gọi trực tiếp (`OnSpellSelected`, `OnEndTurnClicked`...) · nếu chọn mức "Chặt" ở mục 3.3, expose thêm property read-only (`CurrentHpRatio`, `ActiveEffectsSnapshot`...) cho Presentation query lúc init.
- **AC:** `StartCombat()` luôn tạo `DomainEventBus` mới trước khi tạo `CombatState` · gọi `OnSpellSelected()` khi không phải lượt Player (VD đang Enemy Turn) → phải bị chặn ở Controller (guard), không để lọt xuống `TurnController.CastSpell()` rồi mới fail.

## 7.10. Presentation

|View|Đọc từ|Input gọi|AC quan trọng|
|---|---|---|---|
|`HealthBarView`|`DamageEventChannel` (delta) + query 1 lần lúc init|—|Giá trị hiển thị lúc mở scene phải đúng ngay cả khi chưa có event nào bắn (không chờ event đầu tiên mới hiện đúng số)|
|`SpellButtonView`|`SpellDefinition` (icon, cost — đọc Data trực tiếp, hợp lệ vì đây là nội dung tĩnh không phải state)|`CombatController.OnSpellSelected()`|Nút phải tự disable khi `manaCost > currentMp` hoặc spell đang cooldown — đọc state qua Controller (mức Chặt) hoặc Domain (mức Lỏng), không tự đoán|
|`BuffIconView`/`DebuffIconView`|`EffectChangedEventChannel`|—|Neutralize (2 effect cùng lúc biến mất) phải xoá đúng cả 2 icon, không chỉ 1|
|`TurnBannerView`|`TurnPhaseChangedEventChannel`|—|—|
|`CombatResultView`|`CombatEndedEventChannel`|nút "Tiếp tục" → gọi Controller|Phải nhận đúng cả trường hợp Lose xảy ra giữa Start Phase (xem AC của `TurnController` ở 7.6)|