---
**Game:** Wandering Wanderer 
**Author:** DukTofn (Architecture base: Codex, bổ sung: Claude) 
**Last Updated:** 23/07/2026
**Áp dụng cho:** Option A — Layered, Data-driven, Event-driven (ScriptableObject-based)
**Tài liệu này bổ sung, không thay thế** `Architecture & Coding Conventions.md` gốc — đọc file đó trước.

---

## Mục lục

0. [Tóm tắt điều chỉnh so với bản gốc](#0-tóm-tắt-điều-chỉnh-so-với-bản-gốc)
1. [Bảng phân lớp toàn bộ hệ thống theo GDD](#1-bảng-phân-lớp-toàn-bộ-hệ-thống-theo-gdd)
2. [Assembly Reference Graph chính xác](#2-assembly-reference-graph-chính-xác)
3. [Quy tắc giao tiếp giữa các layer](#3-quy-tắc-giao-tiếp-giữa-các-layer)
4. [Thứ tự code](#4-thứ-tự-code)
5. [Checklist trước khi bắt đầu Phase 0](#5-checklist-trước-khi-bắt-đầu-phase-0)

---

# 0. Tóm tắt điều chỉnh so với bản gốc

| # | Vấn đề trong bản gốc | Điều chỉnh |
|---|---|---|
| 1 | Domain "KHÔNG reference UnityEngine" nhưng code ví dụ dùng `Mathf.Pow` — 2 chỗ này mâu thuẫn nhau về mặt asmdef | Domain dùng `System.Math`/`double` thuần, không dùng `Mathf`. Giữ được compile-time guarantee thật sự |
| 2 | "Domain raise event qua interface, không tham chiếu SO" — chưa nói rõ cơ chế | Domain expose 1 `DomainEventBus` thuần C# (plain `event Action<T>`). Controller subscribe bus này rồi forward sang SO Event Channel |
| 3 | Không có `WW.Controllers.asmdef` dù mục 5/6 bản gốc đã ngầm định có layer này | Thêm asmdef thứ 5. Nếu không, Controller code nằm lẫn trong Presentation → Presentation gián tiếp có full quyền vào Data+Domain |
| 4 | "Presentation đọc Domain read-only" không compiler-enforced được | Đưa ra 2 mức nghiêm ngặt, khuyến nghị mức chặt cho hệ thống hay đổi nhiều (combat), mức lỏng cho phần ít rủi ro |
| 5 | `WisdomSlotConfig` chưa rõ nằm ở đâu | Đứng SO riêng ở `Data/`, không lồng trong `CombatBalanceConfig` |
| 6 | Enemy behavior pattern (GDD note "tài liệu riêng, chưa có") chưa có chỗ đứng trong kiến trúc | Đặt sẵn `IEnemyBehaviorStrategy` (Strategy pattern) rỗng ngay từ Phase 3 |
| 7 | Chưa có chiến lược test theo layer, dù mục tiêu là "test nhanh" | Domain = EditMode test (không cần scene, nhanh nhất). Controller/Presentation = Play Mode / test thủ công |
| 8 | `ComboRule` "load từ Data Layer" nhưng chưa nói gom ở đâu | Gom vào 1 `ComboRuleDatabase` SO duy nhất, giống cách `SpellDefinition` được quản lý theo danh sách |
| 9 | Sơ đồ ASCII gốc (Presentation → Event → Domain → Data) dễ hiểu nhầm là reference graph thật | Đây là luồng dữ liệu khái niệm, không phải sơ đồ reference asmdef — xem bảng thật ở mục 2 |

---

# 1. Bảng phân lớp toàn bộ hệ thống theo GDD

## 1.1. Data Layer — `WW.Data` (ScriptableObject, designer chỉnh trực tiếp trên Inspector)

| Asset / Class | Field chính | Nguồn GDD |
|---|---|---|
| `CombatBalanceConfig` (nested: `HpConfig`, `MpConfig`, `ResistanceConfig`, `EffectModifiersConfig`) | `HP_CAP`, `HP_HALF`, `MP_COEFF`, `BASE_MP_RECOVERY`, exponent `p`, % của Enrage/Refreshing/Fortified/Energized/Overdrive/Crystalize/Burn/Drenched/Chilled/Dazed/Detonates/Distracted | Combat Design §2.2.2, §3, §6 |
| `ProgressionBalanceConfig` (nested: `ShopPriceConfig`, `RewardConfig`, `EventPoolConfig`) | Giá theo Rank, Gold reward theo loại combat, tỷ lệ Rank Shop/Reward theo Arc, tỷ lệ + giá trị 10 Event | Progression Design §4, §5, §6 |
| `WisdomSlotConfig` | Ngưỡng WIS mở từng Spell Slot (1→5) | Game Overview §2.1.3 |
| `SpellDefinition` | `element`, `rank`, `manaCost`, `scalingWeights{POT,SPI,WIS,VIT}`, `appliedEffects[]`, `minWisdomToImprint` | Game Overview §2.1.1; Combat Design §2.2.1 |
| `EquipmentDefinition` | `slotType` (Staff/Ring/Book/Garb), `rank`, `attributeBonuses[]`, `passiveEffect` | Game Overview §2.1.6/§2.1.7 |
| `RuneDefinition` | `rank`, `passiveEffect` | Game Overview §2.1.8 |
| `EffectDefinition` | `effectType` enum (Enrage, Refreshing, Fortified, Energized, Burn, Drenched, Chilled, Dazed, Regen, Armor, Distracted), `resolveTime` (Start/End/Instant), `duration`, `magnitude`, `isSelfStackable` | Combat Design §6 |
| `ComboRuleDatabase` (list `ComboRule{requiredA, requiredB, resultCombo}`) | Overdrive, Crystalize, Detonates, Frozen | Game Overview §2.2.1; Combat Design §6 |
| `EnemyDefinition` | `enemyType` (Minion/Elite/Boss), `maxHp`, `potencies[4]`, `resistances[4]`, `spells[]`, `behaviorPattern` (enum, placeholder chờ Enemy Design) | Combat Design §4 |
| `NodeDefinition` | `nodeType` (Combat-Normal/Elite/Boss, Camp, Merchant, Event, Town) | Game Overview §2.1.12 |
| `ArcMapConfig` (1 asset / Arc) | `totalNodes`, `minNodesToEnd`, `minElite`, `minRest`, `minShop`, `minEvent`, `optionalBossCount`, `branchMin/Max` | Progression Design §2.2 |
| `EventDefinition` | `eventType` enum (Windfall...Thief Gang), `category` (+/=/−), payload params (min/max gold, % HP mất...) | Progression Design §6.2 |

> Nguyên tắc: **mọi con số GDD ghi `[TBD]` đều là field trên 1 trong các SO này** — không hardcode ở đâu khác kể cả tạm thời.

## 1.2. Domain Layer — `WW.Domain` (pure C#, không `UnityEngine`, không SO type)

| Class / Struct | Vai trò | Nguồn GDD |
|---|---|---|
| `MainAttributes` (struct) | `POT, SPI, WIS, VIT` | Game Overview §2.1.2 |
| `ScalingWeights`, `SpellRuntimeData`, `EquipmentRuntimeData`, `RuneRuntimeData`, `EnemyRuntimeData`, `EffectRuntimeData`, `ComboRuleRuntimeData`, `NodeRuntimeData`, `EventRuntimeData` | Bản sao runtime của từng SO (do `ToRuntimeData()` sinh ra) | — |
| `PotencyCalculator` (static) | `spell_potency = Σ(weight_attr × attr_value)` | Combat Design §2.2.1 |
| `ResistanceCalculator` (static) | `res_base = 10×(VIT/10)^p`; `damage_reduction = R/(R+90)` | Combat Design §2.2.2 |
| `HPCalculator` (static) | `max_hp = HP_CAP × VIT/(VIT+HP_HALF)` | Combat Design §3.1 |
| `MPCalculator` (static) | `max_mp = MP_COEFF × SPI`; `turn_mp_recovery = BASE + 1.0×SPI` | Combat Design §2.2.3, §3.3 |
| `ElementType` (enum: Water, Fire, Ice, Lightning) + `Beats(a,b)` | Nước>Lửa>Băng>Sét>Nước | Game Overview §2.2.2 |
| `ICombatant`, `PlayerCombatant`, `EnemyCombatant` | Adapter chung cho Player (tính từ MainAttributes) và Enemy (đọc thẳng từ định nghĩa) | Combat Design §4 |
| `ActiveEffect` | Effect đang active trên 1 combatant, có `duration`, `isSelfStackable` | Combat Design §6 |
| `ArmorStack` + `DamageApplier` | `value`, `duration`, `hurt_order`; resolve damage theo `hurt_order` cao nhất trước | Combat Design §3.2 |
| `EquipmentLoadout` (4 slot), `RuneLoadout` (4 socket) | Trạng thái trang bị/rune hiện tại của Player | Game Overview §2.1.7, §2.1.9 |
| `IStartPhaseStep` + 7 implementation (`MpRecoveryStep`, `FrozenCheckStep`, `CrystalizeCheckStep`, `RegenStep`, `BurnStep`, `RemainingStatusStep`, `ComboCheckStep`) | Đúng thứ tự resolve Start Phase | Combat Design §5.1.1 |
| `IEndPhaseStep` + 5 implementation (`EffectDurationTickStep`, `EffectExpiryStep`, `ArmorDurationTickStep`, `ArmorExpiryStep`, `SpellCooldownTickStep`) | Đúng thứ tự resolve End Phase | Combat Design §5.1.3 |
| `StartPhaseRunner`, `EndPhaseRunner` | Chạy list step theo thứ tự cố định, thêm step mới không sửa step cũ | Combat Design §5.1 |
| `EffectResolver` | Áp dụng element interaction rule (khắc chế / neutralize cùng nguyên tố) | Game Overview §2.2.2 |
| `ComboEvaluator` | Check + trigger Overdrive/Crystalize/Detonates/Frozen từ `ComboRuleRuntimeData[]` | Combat Design §6 |
| `IEnemyBehaviorStrategy` (interface, **placeholder**) + `RandomBehaviorStrategy` (impl tạm) | Enemy chọn spell nào để cast | Combat Design §4 — *chờ Enemy Design GDD* |
| `DomainEventBus` + payload struct (`DamageDealtPayload`, `EffectAppliedPayload`...) | Điểm nối Domain → Controller — xem mục 3.1 | — (mới thêm) |
| `CombatState` | Player + enemy list + turn hiện tại + phase hiện tại | Combat Design §5 |
| `TurnController` | `CastSpell()`, `AdvanceTurn()` — nơi Controller gọi vào | Game Overview §2.3.2 |
| *(Progression, thứ yếu cho MVP)*: `MapGraph`, `MapGraphGenerator`, `SpellSlotUnlockCalculator` (dùng `WisdomSlotConfig`), `ShopInventoryGenerator`, `ShopService`, `RewardRoller`, `IEventEffect` + 10 impl, `EventResolver` | Sinh map, shop, reward, event | Progression Design toàn bộ |

## 1.3. Event Layer — `WW.Events` (SO Event Channel, concrete class — không cần generic)

| Channel | Payload | Bắn khi |
|---|---|---|
| `DamageEventChannel` | target, element, amount | `TurnController` gây damage |
| `EffectChangedEventChannel` | target, effectType, `changeType` (Applied/Expired/Neutralized) | Effect apply/hết hạn/bị giải |
| `TurnPhaseChangedEventChannel` | `TurnPhase` mới | Start/Action/End Phase đổi |
| `CombatEndedEventChannel` | `CombatResult` (Win/Lose) | Win/Lose condition true |
| `SpellCastEventChannel` | caster, spellId | Ngay khi cast (tách khỏi Damage vì spell buff không gây damage) |
| *(sau, cho Progression)*: `NodeEnteredEventChannel`, `ShopOpenedEventChannel`, `GoldChangedEventChannel` | — | — |

> Quyết định luôn: **dùng concrete class, không dùng `GameEventSO<T>` generic** (mục 10 bản gốc để mở — mình chốt theo đúng đề xuất "bắt đầu bằng concrete" trong chính bản gốc, vì generic khó debug trên Inspector và team có non-tech).

## 1.4. Controller Layer — `WW.Controllers` *(asmdef mới thêm)*

| Class | Vai trò |
|---|---|
| `CombatController` (MonoBehaviour) | Sở hữu `CombatState`, subscribe `DomainEventBus`, forward sang SO Event Channel, expose method public cho input (`OnSpellSelected`...) |
| `MapController` / `RunController` *(sau)* | Điều phối di chuyển giữa Node, sở hữu `MapGraph` |
| `ShopController` *(sau)* | Mở/đóng Shop, gọi `ShopService` |

## 1.5. Presentation Layer — `WW.Presentation`

| View | Đọc từ | Ghi (input) |
|---|---|---|
| `HealthBarView`, `MpBarView`, `ArmorStackView` | `DamageEventChannel` (delta) + query 1 lần lúc init | — |
| `SpellButtonView`, `SpellSlotView` | `SpellDefinition` (Data, để hiển thị icon/cost) | gọi `CombatController.OnSpellSelected()` |
| `BuffIconView` / `DebuffIconView` | `EffectChangedEventChannel` | — |
| `TurnBannerView` | `TurnPhaseChangedEventChannel` | — |
| `CombatResultView` | `CombatEndedEventChannel` | nút "Tiếp tục" → gọi Controller |
| *(sau)* `NodeMapView` (render fog of war), `ShopItemView`, `EventPopupView` | — | — |

---

# 2. Assembly Reference Graph chính xác

Sơ đồ ASCII ở bản gốc (Presentation → Event → Domain → Data) là **luồng dữ liệu khái niệm**, không phải sơ đồ reference asmdef thật. Reference graph thật (5 asmdef):

| Assembly | Được reference tới | **Không được** reference |
|---|---|---|
| `WW.Data` | *(không reference project nào khác, chỉ UnityEngine)* | WW.Domain, WW.Events, WW.Controllers, WW.Presentation |
| `WW.Domain` | *(không reference gì trong project)* | **UnityEngine** (dùng `System.Math`), WW.Data (chỉ nhận runtime struct, không nhận SO type), WW.Events, WW.Controllers, WW.Presentation |
| `WW.Domain.Tests` (EditMode) | WW.Domain, NUnit | UnityEngine (không cần, vì Domain không cần) |
| `WW.Events` | WW.Domain (chỉ để dùng type cho payload) | WW.Data, WW.Controllers, WW.Presentation |
| `WW.Controllers` | WW.Data, WW.Domain, WW.Events | WW.Presentation (Controller không được biết View cụ thể nào tồn tại) |
| `WW.Presentation` | WW.Events, WW.Controllers, WW.Data *(chỉ để đọc field hiển thị như icon/tên — xem mục 3.3)* | *(xem mục 3.3 về WW.Domain)* |

**Vì sao thêm `WW.Controllers`:** nếu Controller code nằm chung asmdef với Presentation (như bản gốc ngầm định), Presentation sẽ *transitively* có quyền gọi thẳng Data + Domain thông qua asmdef reference của Controller — đúng thứ mà cả hệ thống asmdef được dựng lên để chặn. Tách riêng thì Presentation chỉ còn đường hợp lệ duy nhất để mutate state là gọi qua `WW.Controllers`.

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

| Mức | Cách làm | Khi nên dùng |
|---|---|---|
| **Lỏng** (nhanh hơn, ít boilerplate) | Presentation reference thẳng `WW.Domain`, đọc property (`combatant.CurrentHP`) khi cần. Quy ước bắt buộc: method mutate trong Domain luôn đặt tên là **verb hành động** (`Apply`, `Tick`, `Cast`, `Resolve`), method đọc luôn là **property/`Get*`** — review bằng mắt khi PR | Progression layer, phần ít đổi, ít người đụng |
| **Chặt** (compiler enforce thật) | Presentation **không** reference `WW.Domain`. Controller expose sẵn property/DTO read-only (VD `public float CurrentHpRatio => _state.Player.CurrentHP / _state.Player.MaxHP;`) | Combat layer — nơi 2-4 người cùng đụng code, GDD note rõ "chắc chắn sẽ mở rộng" |

**Khuyến nghị:** dùng mức **Chặt** cho Combat Controller (đúng tinh thần asmdef ban đầu của chính bạn — "biến convention thành lỗi compile"), mức **Lỏng** chấp nhận được ở Progression vì đó là phần rút gọn, ít rủi ro hơn cho MVP. Đây là điểm có đánh đổi thật (thêm vài property wrapper ở Controller) — không phải đúng/sai tuyệt đối, bạn có thể chọn Lỏng cho cả hai nếu ưu tiên tốc độ hơn.

## 3.4. Bảng tổng hợp quyền gọi

| Từ \ Đến | Data | Domain | Events | Controllers | Presentation |
|---|---|---|---|---|---|
| **Data** | — | ❌ | ❌ | ❌ | ❌ |
| **Domain** | chỉ runtime struct (`ToRuntimeData()` output) | — | ❌ (raise qua `DomainEventBus` thuần C#) | ❌ | ❌ |
| **Events** | ❌ | dùng type cho payload | — | ❌ | ❌ |
| **Controllers** | ✅ đọc SO | ✅ gọi mutate + subscribe `DomainEventBus` | ✅ `.Raise()` | — | ❌ (không được biết View cụ thể) |
| **Presentation** | ✅ đọc (icon, tên, cost để hiển thị) | mức Lỏng: ✅ đọc / mức Chặt: ❌ | ✅ subscribe | ✅ gọi method input | — |

---

# 4. Thứ tự code

Nguyên tắc chọn thứ tự: **logic thuần trước, wiring Unity sau, visual cuối cùng** — vì logic thuần test bằng EditMode (mili giây, không cần scene), còn Play Mode chỉ nên dùng để verify wiring/cảm giác chơi, không phải để bắt lỗi công thức.

| Phase | Nội dung | Layer | Test | Điều kiện qua Phase |
|---|---|---|---|---|
| **0** | 5 asmdef (Data, Domain, Domain.Tests, Events, Controllers, Presentation) + folder structure + git | — | Cố ý `using UnityEngine;` trong `WW.Domain` → phải thấy lỗi compile | Lỗi compile xuất hiện đúng như kỳ vọng |
| **1** | `CombatBalanceConfig`, `ProgressionBalanceConfig`, `WisdomSlotConfig` + `SpellDefinition`/`EquipmentDefinition`/`RuneDefinition`/`EnemyDefinition`/`EffectDefinition` (field đầy đủ, `ToRuntimeData()` stub) | Data | Tạo được asset trên Inspector, không lỗi | Designer bắt đầu điền được data thật song song với Phase 2+ |
| **2** | `PotencyCalculator`, `ResistanceCalculator`, `HPCalculator`, `MPCalculator`, `ElementType.Beats()` | Domain (formula thuần) | EditMode: so khớp đúng bảng tham khảo GDD (VD VIT=50→res_base=75.8; VIT=90→max_hp=500) | Mọi bảng tham khảo trong Combat Design pass test |
| **3** | `MainAttributes`, `ICombatant`, `PlayerCombatant`, `EnemyCombatant`, `ActiveEffect`, `ArmorStack`+`DamageApplier`, `IEnemyBehaviorStrategy` (placeholder) | Domain (entity) | EditMode: damage tràn đúng thứ tự `hurt_order` (ví dụ §3.2 GDD Combat Design) | Ví dụ Armor 2 stack trong GDD cho kết quả đúng |
| **4** | `IStartPhaseStep`×7, `IEndPhaseStep`×5, `StartPhaseRunner`, `EndPhaseRunner` | Domain (turn engine) | EditMode: log thứ tự step chạy, so với bảng GDD §5.1.1/§5.1.3 | Thứ tự resolve khớp 100% bảng GDD |
| **5** | `EffectResolver` (element interaction), `ComboEvaluator` (đọc `ComboRuleRuntimeData[]`) | Domain (effect/combo — điểm nóng nhất) | EditMode: case Nước giải Lửa; case Enrage+Energized→Overdrive kích hoạt đúng lượt sau | Toàn bộ bảng §2.2.2 (Game Overview) + §6 (Combat Design) có test case |
| **6** | `DomainEventBus`, `CombatState`, `TurnController.CastSpell()` | Domain (glue nội bộ) | EditMode integration: cast 1 spell lửa lên bản thân → subscribe `DomainEventBus` ngay trong test, verify payload đúng | Luồng "cast spell" ví dụ §8 bản gốc chạy được **hoàn toàn không cần Unity** |
| **7** | 5-6 SO Event Channel cụ thể cho Arc 1 | Events | Compile only | — |
| **8** | `CombatController` (StartCombat, OnSpellSelected, forward bus→channel) | Controllers | **Play Mode đầu tiên**: scene trống, hardcode 1 enemy, gọi `OnSpellSelected` qua Context Menu tạm, xem `Debug.Log` ở listener | Event Channel nhận đúng payload trong Play Mode |
| **9** | `HealthBarView`, `SpellButtonView`, `BuffIconView`, `TurnBannerView`, `CombatResultView` | Presentation | Play Mode: chơi thử combat 1vs1 bằng UI thật | Thắng/thua 1 trận qua UI, không qua code debug |
| **10** | Điền data thật Arc 1 (10-12 spell theo scope MVP, equipment/rune/enemy tương ứng) | Data (content) | Playtest thủ công | Chơi hết Arc 1 combat loop không crash |
| **11** *(sau, rút gọn cho MVP)* | `NodeDefinition`, `ArcMapConfig` (map cố định nhỏ, chưa cần full generation algorithm), `ShopController`, `RewardRoller`, `EventResolver` (10 event) | Data+Domain+Controllers+Presentation | Play Mode | Đi được từ đầu Arc 1 đến Boss cuối |

> Phase 11 cố tình để cuối và "rút gọn" — đúng scope MVP đã ghi ở đầu file gốc ("Progression rút gọn"), và vì thuật toán sinh layout map tự GDD Progression Design đã note là "📄 Tech Design — chưa có" ở mục 7.

---

# 5. Checklist trước khi bắt đầu Phase 0

- [ ] Chốt dùng `System.Math` (không `Mathf`) trong toàn bộ `WW.Domain` — ghi thành convention mục 7 file gốc.
- [ ] 5 asmdef (không phải 4) — thêm `WW.Controllers`.
- [ ] Chọn giá trị **tạm** cho `BASE_MP_RECOVERY` và exponent `p` (đang `[TBD]` trong GDD) — không cần đúng số cuối, chỉ cần có số để Phase 2 viết test được. Balancing thật làm sau, không block code.
- [ ] Quyết định mức Lỏng/Chặt cho Presentation↔Domain (mục 3.3) — áp dụng nhất quán, đừng để mỗi View một kiểu.
- [ ] Tạo `WW.Domain.Tests.asmdef` (EditMode, reference `WW.Domain` + NUnit) song song với `WW.Domain` ngay từ Phase 2, đừng để dồn viết test sau cùng.