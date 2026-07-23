---

**Game:** Wander **Author:** DukTofn **Last Updated:** 23/07/2026

---

## Mục lục

1. [Document Direction](#1-document-direction)
2. [Targeting Model](#2-targeting-model)
    - [2.1. Target Type](#21-target-type)
    - [2.2. Friendly/Hostile & Elemental Auto-Status](#22-friendlyhostile--elemental-auto-status)
3. [Spell Data Schema](#3-spell-data-schema)
    - [3.1. SpellDefinition](#31-spelldefinition)
    - [3.2. SpellEffect](#32-spelleffect)
    - [3.3. Scaling — Independent & Derived](#33-scaling--independent--derived)
    - [3.4. Resolve Order trong 1 Spell](#34-resolve-order-trong-1-spell)
4. [Spell Slot, Imprint & Acquisition](#4-spell-slot-imprint--acquisition)
    - [4.1. Spell Slot](#41-spell-slot)
    - [4.2. Imprint / Forget](#42-imprint--forget)
    - [4.3. Acquisition Loop](#43-acquisition-loop)
5. [Rank & Power Curve](#5-rank--power-curve)
6. [Casting Resolution (Action Phase)](#6-casting-resolution-action-phase)
    - [6.1. Multi-cast trong 1 lượt](#61-multi-cast-trong-1-l%C6%B0%E1%BB%A3t)
    - [6.2. Cooldown](#62-cooldown)
    - [6.3. UI/UX — Drag & Target](#63-uiux--drag--target)
7. [Edge Case & Combo Interaction](#7-edge-case--combo-interaction)
    - [7.1. Combo Trigger — Event-driven](#71-combo-trigger--event-driven)
    - [7.2. Target hợp lệ khi Resolve](#72-target-h%E1%BB%A3p-l%E1%BB%87-khi-resolve)
    - [7.3. Win Condition Timing](#73-win-condition-timing)
    - [7.4. Element Neutralize vs Re-apply cùng Element](#74-element-neutralize-vs-re-apply-c%C3%B9ng-element)
    - [7.5. Effect Re-apply / Stackability khác](#75-effect-re-apply--stackability-kh%C3%A1c)
8. [Ghi chú & Mục cần hoàn thiện](#8-ghi-ch%C3%BA--m%E1%BB%A5c-c%E1%BA%A7n-ho%C3%A0n-thi%E1%BB%87n)

---

# 1. Document Direction

Tài liệu này định nghĩa toàn bộ hệ thống Spell: targeting model, data schema, vòng đời sở hữu/trang bị (Slot – Imprint – Acquisition), power curve theo Rank, và cách casting resolve trong combat.

**Không bao gồm:**

- Nội dung cụ thể của từng Spell (số liệu thật, danh sách đầy đủ) — xem tài liệu **Spell Design** riêng khi được biên soạn.
- Cấu trúc Turn / Resolve Phase dùng chung toàn bộ combat — xem _Combat Design, Section 5_.
- Bảng Effect (Buff/Debuff) và cơ chế combo (Overdrive/Crystalize/Frozen/Detonates) — xem _Combat Design, Section 6_.
- Kiến trúc dữ liệu & runtime (ScriptableObject, state, pipeline) — sẽ nằm ở **TDD — Spell System** riêng.

---

# 2. Targeting Model

## 2.1. Target Type

11 Target Type, chia 2 nhóm đối xứng (Enemy-side / Ally-side) cộng `Self` riêng biệt:

| Nhóm       | Target Type                 | Mô tả                                  |
| ---------- | --------------------------- | -------------------------------------- |
| —          | **Self**                    | Nhắm chính người cast                  |
| Enemy-side | Single Enemy                | 1 Enemy được chọn                      |
| Enemy-side | All Enemies                 | Toàn bộ Enemy                          |
| Enemy-side | Primary Enemy + Sub Enemies | 1 Enemy chính + các Enemy phụ (splash) |
| Enemy-side | Random Enemy                | 1 Enemy ngẫu nhiên                     |
| Enemy-side | Random Multiple Enemies     | N Enemy ngẫu nhiên                     |
| Ally-side  | Single Ally                 | 1 Ally được chọn                       |
| Ally-side  | All Allies                  | Toàn bộ Ally                           |
| Ally-side  | Primary Ally + Sub Allies   | 1 Ally chính + các Ally phụ            |
| Ally-side  | Random Ally                 | 1 Ally ngẫu nhiên                      |
| Ally-side  | Random Multiple Allies      | N Ally ngẫu nhiên                      |

> **Target Type là hệ generic dùng chung cho cả Player và Enemy** (vì Enemy dùng chung spell pool với Player) — không hardcode riêng cho từng phía. "Ally"/"Enemy" luôn hiểu **tương đối theo phe của người cast**: Enemy cast 1 Spell Ally-type sẽ nhắm các Enemy khác trong cùng trận, không phải Player.
> 
> **Ally-side khi Player cast:** Player hiện tại solo (không companion) nên Ally-side luôn resolve về chính Player. Hệ thống vẫn thiết kế generic/mở cho tương lai, nhưng code path hiện tại chỉ cần đúng cho trường hợp solo. Vì pool Ally-side **luôn tính cả bản thân người cast**, Ally-targeting không bao giờ zero-valid-target (xem thêm 7.2).
> 
> **Primary + Sub:** "Primary" là 1 target chính chịu full effect, "Sub" là các target phụ ăn theo — thường dùng **Derived scaling** (xem 3.3). Ví dụ kinh điển: damage chính vào Primary Enemy, splash % damage đó sang các Sub Enemies xung quanh.

## 2.2. Friendly/Hostile & Elemental Auto-Status

Mỗi **Effect** trong 1 Spell **tự có TargetType riêng** — không dùng chung 1 TargetType cho toàn bộ Spell. Nhờ vậy 1 Spell có thể vừa Deal DMG lên Enemy vừa Heal lên Self trong cùng 1 lần cast (VD: spell "hút máu" — xem ví dụ đầy đủ ở 3.3).

Quan hệ giữa effect và target chỉ có **2 nhóm**, không cần bảng 3 chiều:

- **Friendly** (Self + Ally) → Buff
- **Hostile** (Enemy) → Debuff

**Cơ chế trigger — tag-based, không quan tâm magnitude:**

Bất kỳ Effect nào (Deal DMG, Heal, Gain Armor... — không riêng Deal DMG) mang tag DamageType đúng nguyên tố của Spell đều trigger auto-status tương ứng, **kể cả khi effect đó không thật sự gây damage** (magnitude/damage = 0). Ví dụ: 1 Spell Nước tự cast lên bản thân để tạo Armor (Effect kind = Gain Armor, không phải Deal DMG) **vẫn** tự động cộng thêm Refreshing — vì effect đó vẫn mang tag Water, chỉ cần đúng tag là đủ, độ lớn không quan trọng.

**Bảng lookup `(Element, Friendly/Hostile) → Status`:**

|Element|Friendly (Self/Ally) → Buff|Hostile (Enemy) → Debuff|
|---|---|---|
|Fire|Enrage|Burn|
|Water|Refreshing|Drenched|
|Ice|Fortified|Chilled|
|Lightning|Energized|Dazed|

> Với multi-target (All Enemies/Random Multiple...), status áp dụng **độc lập theo từng target**, dựa trên damage instance THỰC TẾ landing lên target đó — không phải theo "spell này nhắm ai" nói chung. Mỗi target trúng damage Fire tự nhận Burn riêng, không phụ thuộc target khác.
> 
> Primary và Sub luôn cùng DamageType với nhau, vì 1 Spell chỉ mang **1 Element duy nhất**, áp dụng cho MỌI Effect trong Spell đó (không có Spell Non-elemental — mọi Spell, kể cả dạng Heal/Shield/Utility thuần, đều gắn đúng 1 trong 4 nguyên tố).

Chi tiết combo (Overdrive/Crystalize/Frozen/Detonates) sinh ra từ các status này — xem _Combat Design, Section 5 & 6_.

---

# 3. Spell Data Schema

```
SpellDefinition {
  id, name, rank              // Rank I / II / III — xem Section 5
  element                     // Fire | Water | Ice | Lightning — DUY NHẤT, áp cho MỌI Effect trong Spell
  manaCost, cooldown          // mỗi Spell tự set tự do, Rank chỉ định hướng (xem Section 5)
  minWisdomToImprint
  effects: [SpellEffect]      // 1..N effect, resolve theo Section 3.4
}
 
SpellEffect {
  kind: DealDamage | Heal | GainArmor | ApplyEffect | RemoveEffect | RecoverMP
  targetType: Self | SingleEnemy | AllEnemies | PrimaryEnemy+SubEnemies
            | RandomEnemy | RandomMultipleEnemies
            | SingleAlly | AllAllies | PrimaryAlly+SubAllies
            | RandomAlly | RandomMultipleAllies
 
  scaling:
      Independent { basePower, scalingWeights{POT,SPI,WIS,VIT} }
        → magnitude = basePower + Σ(weight_attr × attr_value)
    | Derived { sourceEffectRef (chỉ trỏ tới 1 Effect Independent khác trong CÙNG Spell), percentage }
        → magnitude = percentage × giá_trị_đã_resolve(sourceEffect)
 
  // riêng ApplyEffect: statusToApply (Regen / Distracted / Burn / ...)
  // riêng RemoveEffect: filter = Specific(statusId) | Category(Buff/Debuff/All) | Random
}
```

## 3.1. SpellDefinition

Cấp cao nhất, đại diện 1 Spell hoàn chỉnh. `element` khai báo ở cấp này (không phải cấp Effect) vì 1 Spell luôn chỉ mang đúng 1 nguyên tố cho mọi Effect bên trong. `manaCost`/`cooldown`/`minWisdomToImprint` cũng ở cấp Spell — dùng chung cho toàn bộ lần cast, không tách theo từng Effect.

## 3.2. SpellEffect

Đơn vị nhỏ nhất trong 1 Spell. Mỗi Spell chứa **list 1..N Effect**, mỗi Effect có `kind` và `targetType` **độc lập** với các Effect khác trong cùng Spell — đây là điểm khác biệt quan trọng so với thiết kế "1 Spell = 1 con số damage": cho phép 1 Spell vừa đánh Enemy vừa buff/heal chính mình cùng lúc.

## 3.3. Scaling — Independent & Derived

Magnitude của 1 Effect có 2 nhánh:

- **Independent** — không phụ thuộc Effect khác: `magnitude = basePower + Σ(weight_attr × attr_value)` — chính là `spell_potency` đã định nghĩa ở _Combat Design §2.2.1_, cộng thêm `basePower` riêng của Effect đó.
- **Derived** — magnitude là % giá trị **đã resolve** của 1 Effect khác trong cùng Spell: `magnitude = percentage × giá_trị_đã_resolve(sourceEffect)`. Derived **chỉ được tham chiếu Effect loại Independent**, không được tham chiếu 1 Derived khác (cấm chain) — tránh vòng lặp/phụ thuộc phức tạp.

> **Ví dụ "hút máu":** Effect 1 (Independent, Deal DMG, target = Primary Enemy) đánh damage chính → Effect 2 (Derived, Deal DMG, % của Effect 1, target = Sub Enemies) splash sang các target phụ → Effect 3 (Derived, Heal, % của Effect 1, target = Self) hồi máu lại cho caster. Cả 3 Effect có TargetType riêng biệt (Enemy / Enemy / Self) trong cùng 1 Spell.
> 
> `basePower` **không** theo bảng Rank chung — mỗi Effect tự set base riêng (không phải cấp Spell hay cấp Rank). Rank chỉ là nhãn định hướng balancing tương đối, không ép công thức cứng — cùng tinh thần với `mana_cost`/`cooldown` tự do theo từng Spell (xem Section 5).
> 
> `scalingWeights` dùng **CHUNG 1 công thức cho mọi EffectKind** — Damage/Heal/GainArmor/RecoverMP đều tính Independent magnitude theo cùng công thức, không có attribute ưu tiên riêng theo loại effect.

## 3.4. Resolve Order trong 1 Spell

Effect resolve theo **2 tầng ưu tiên**:

1. **Toàn bộ Effect Independent** resolve trước — theo đúng thứ tự khai báo trong list giữa các Independent với nhau.
2. **Toàn bộ Effect Derived** resolve sau — đọc giá trị đã resolve ở bước 1, theo đúng thứ tự khai báo trong list giữa các Derived với nhau.

> **Lý do tách 2 tầng:** Derived bắt buộc đọc giá trị đã tính xong của 1 Independent (3.3), nên Independent luôn phải resolve trước để đảm bảo giá trị sẵn sàng — bất kể effect đó được khai báo ở vị trí nào trong list. Trong thực tế, đa số Spell sẽ khai báo Independent trước Derived theo trình tự tự nhiên (dmg chính → splash/heal ăn theo), nên "theo thứ tự khai báo" và "Independent trước, Derived sau" hầu như luôn trùng nhau; tầng ưu tiên này chỉ cần thiết để xử lý đúng cả trường hợp lỡ khai báo lệch thứ tự.

**Win Condition check sau MỖI Effect** (áp dụng xuyên suốt cả tầng Independent lẫn Derived — xem thêm 7.3): nếu 1 Effect vừa resolve khiến điều kiện thắng/thua đủ điều kiện, dừng resolve ngay lập tức — các Effect còn lại (kể cả Derived cùng Spell) **không** chạy nữa.

---

# 4. Spell Slot, Imprint & Acquisition

## 4.1. Spell Slot

- Người chơi bắt đầu với **1 Slot**, tối đa **5 Slot** khi WIS đạt max (theo _Game Overview §2.1.3_).
- Slot tự động mở khi WIS hiện tại đạt ngưỡng tương ứng (`WisdomSlotConfig`) — không cần thao tác, không tốn Gold.
- Ngưỡng WIS cụ thể cho từng Slot (`WisdomSlotConfig`) vẫn `[TBD]` Balancing.

> ⚠️ **Cần đối chiếu:** vòng Q&A trước từng để ngỏ "chưa quyết" cho cả **số lượng** Slot (không chỉ ngưỡng mở từng slot), trong khi Game Overview đã ghi cứng 1 start / 5 max. Doc này giữ nguyên 1/5 làm nguồn hiện tại vì đã publish, nhưng flag lại để xác nhận đây có còn là số cuối hay không.

## 4.2. Imprint / Forget

- Đổi Spell trong Slot (Imprint/Forget) **tự do bất cứ lúc nào NGOÀI combat**, qua menu quản lý Spell — không giới hạn số lần, không tốn phí.
- Điều kiện Imprint: `WIS` hiện tại của nhân vật ≥ `minWisdomToImprint` của Spell đó.
- `Forget` không đòi hỏi WIS (theo _Game Overview §2.1.3_).

## 4.3. Acquisition Loop

- Spell mới nhận (Shop/Combat Reward) nhưng hết Slot hoặc chưa đủ WIS để Imprint → vẫn **giữ lại trong inventory không giới hạn**, Imprint bất cứ lúc nào sau khi đủ điều kiện — không mất, không hết hạn.
- Nguồn nhận Spell: Magic Shop (5 random/lần trong 15 mặt hàng) hoặc Combat Reward (offer 1-trong-3: Equipment/Spell/Rune) — xem _Progression Design §4, §5_.

---

# 5. Rank & Power Curve

- 3 Rank (I/II/III) — Rank cao hơn = effect mạnh hơn, **không** theo công thức cứng.
- `basePower`, `mana_cost`, `cooldown` đều **tự set tự do theo từng Spell/Effect** — không có range/formula chung bắt buộc theo Rank. Rank chỉ mang tính định hướng balancing tương đối (Rank III nên mạnh hơn Rank I, nhưng không có công thức ép buộc).
- "Cơ chế đặc biệt" của Rank III (đề cập ở _Progression Design §3.2_) — chưa có rule chung, để ngỏ cho từng Spell tự do sáng tạo (VD: mỗi Rank III có thể mang cơ chế unique riêng, không cần theo template chung).

---

# 6. Casting Resolution (Action Phase)

## 6.1. Multi-cast trong 1 lượt

- Có thể cast **nhiều Spell khác nhau** trong 1 Action Phase, miễn đủ MP.
- Mỗi Spell chỉ được cast **tối đa 1 lần/lượt**, dù đủ MP và Cooldown cho phép — không thể spam cùng 1 Spell nhiều lần trong 1 lượt.

## 6.2. Cooldown

- Cooldown set về giá trị max ngay khi cast.
- Tick giảm ở nhóm Tick của Resolve Phase (bước 5 — xem _Combat Design §5.1.1_), cùng nhóm với Effect duration tick và Armor duration tick.

## 6.3. UI/UX — Drag & Target

- Spell cần target cụ thể (Single Enemy/Single Ally...): drag spell vào đúng target đó để confirm cast.
- Spell không cần chọn target cụ thể (Self/All Enemies/All Allies...): vẫn drag, nhưng thả vào **bất kỳ đâu** trong vùng combat cũng được — vị trí thả không mang ý nghĩa xác định target, chỉ là gesture "confirm cast". Target thật sự luôn do `TargetType` của từng Effect quyết định (2.1, 3.2).

---

# 7. Edge Case & Combo Interaction

## 7.1. Combo Trigger — Event-driven

- Combo (Overdrive/Crystalize/Frozen/Detonates) trigger theo kiểu **event-driven**: ngay sau khi 1 Effect apply xong buff/debuff mới lên 1 target — bất kể đang ở Action Phase hay Resolve Phase — hệ thống check ngay target đó có đủ cặp điều kiện combo hay không.
- **Không** phải bước cố định trong Resolve Order (đã sửa lại ở _Combat Design §5.1.1_).
- **Detonates là single-target** — chỉ nổ lên chính target đang mang combo Burn + Dazed, không phải AoE lên toàn bộ phe địch.

## 7.2. Target hợp lệ khi Resolve

- **Zero valid target** tại thời điểm chọn (VD: Random Enemy nhưng hết Enemy sống) → **chặn cast từ đầu**; UI validate toàn bộ Effect list trước khi cho phép thả spell, không fizzle sau khi đã cast.
- Ally-side pool luôn tính cả bản thân người cast (2.1) → về cấu trúc không bao giờ zero-target cho Ally-targeting.
- Phía Enemy nhắm Hostile side (Player) cũng luôn có ít nhất Player còn sống (Player chết = combat đã kết thúc) → cũng không bao giờ zero-target.
- Target type cần **nhiều hơn 1 target** (Random Multiple Enemies/Allies, Primary+Sub) nhưng số lượng hợp lệ hiện có **ít hơn** số Spell muốn (VD: định nghĩa hit 3 Random Enemies nhưng chỉ còn 2 sống) → **graceful degrade**: vẫn cast, hit đúng số lượng hiện có, không tính là invalid miễn ≥ 1 target hợp lệ.

## 7.3. Win Condition Timing

Check Win/Lose condition ngay sau **MỖI Effect** resolve (cả Independent lẫn Derived — xem 3.4) — nếu đủ điều kiện thắng/thua, dừng resolve ngay lập tức, bỏ qua phần Effect/Spell còn lại trong chuỗi (kể cả các Spell khác đang chờ trong multi-cast). Đây là trade-off chủ động: ưu tiên kết thúc combat gọn gàng hơn đảm bảo mọi effect luôn chạy hết hoàn chỉnh — VD: lifesteal/splash phía sau có thể "hụt" nếu effect trước đã giết địch cuối cùng.

## 7.4. Element Neutralize vs Re-apply cùng Element

- Rule Neutralize (_Combat Design §2.2.2_ — "Effect A đang active, apply buff/debuff cùng nguyên tố A → cả hai bị giải trừ") **chỉ áp dụng khi KHÁC polarity** (buff dispel debuff cùng nguyên tố, hoặc ngược lại).
- Re-apply **CÙNG polarity** (VD: đã có Enrage active, cast thêm 1 phép Lửa lên chính mình nữa) → **no-op**, không neutralize — effect cũ giữ nguyên, khớp với marker `non-self-stackable` đã có sẵn trong bảng Effect Design.

## 7.5. Effect Re-apply / Stackability khác

- `Distracted` là **non-self-stackable** (đã bổ sung marker vào _Combat Design §6.2_) — apply lại khi đang active chỉ refresh/no-op, không cộng dồn % mana_cost.

---

# 8. Ghi chú & Mục cần hoàn thiện

|Mục|Trạng thái|Ghi chú|
|---|---|---|
|Số lượng Spell Slot khởi đầu/tối đa|⚠️ Cần đối chiếu|Game Overview ghi cứng 1 start / 5 max, nhưng vòng Q&A trước để ngỏ "chưa quyết" — cần xác nhận số nào là bản cuối.|
|`WisdomSlotConfig` (ngưỡng WIS mở từng Slot)|⏳ Balancing|Chưa có giá trị cụ thể.|
|`basePower`, `mana_cost`, `cooldown` cụ thể từng Spell/Effect|⏳ Balancing|Không có formula/range chung theo Rank — mỗi Spell/Effect tự set.|
|`RemoveEffect` filter `Random`|❓ Chưa xác nhận trực tiếp|Xuất hiện trong bản schema tổng hợp cuối cùng của vòng Q&A trước nhưng không có câu hỏi riêng xác nhận — cần chốt lại có giữ hay bỏ nhánh này.|
|Rank III "cơ chế đặc biệt"|🔜 Mở, tự do theo từng Spell|Không cần rule chung — thiết kế riêng lẻ khi làm nội dung Spell cụ thể.|
|Nội dung Spell cụ thể (list đầy đủ, số liệu thật)|📄 Tài liệu riêng|Doc này chỉ định nghĩa schema & rule chung — xem **Spell Design** khi biên soạn.|
|TDD — kiến trúc dữ liệu & runtime (ScriptableObject, state, pipeline)|📄 Category 6, chưa làm|Bước tiếp theo sau khi GDD này được chốt.|