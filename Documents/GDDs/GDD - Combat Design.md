---

---
---

**Game:** Wander 
**Author:** DukTofn 
**Last Updated:** 23/07/2026

---

## Mục lục

1. [Document Direction](#1-document-direction)
2. [Attributes](#2-attributes)
    - [2.1. Main Attributes](#21-main-attributes)
    - [2.2. Sub Attributes](#22-sub-attributes)
        - [2.2.1. Potencies](#221-potencies)
        - [2.2.2. Resistances](#222-resistances)
        - [2.2.3. MP Recovery](#223-mp-recovery)
3. [HP, Armor & MP](#3-hp-armor--mp)
    - [3.1. HP](#31-hp)
    - [3.2. Armor](#32-armor)
    - [3.3. MP](#33-mp)
4. [Enemy Attributes](#4-enemy-attributes)
5. [Turn Logic & Structure](#5-turn-logic--structure)
    - [5.1. Turn Structure](#51-turn-structure)
        - [5.1.1. Resolve Phase](#511-resolve-phase)
        - [5.1.2. Action Phase](#512-action-phase)
6. [Effect Design](#6-effect-design)
    - [6.1. Buffs](#61-buffs)
    - [6.2. Debuffs](#62-debuffs)
7. [Ghi chú & Mục cần hoàn thiện](#7-ghi-ch%C3%BA--m%E1%BB%A5c-c%E1%BA%A7n-ho%C3%A0n-thi%E1%BB%87n)

---

# 1. Document Direction

Tài liệu này cung cấp định nghĩa và cách hoạt động chi tiết của các thành phần, cơ chế và tương tác trong combat.

Không bao gồm các yếu tố về tiến trình game hoặc tiến trình sức mạnh của người chơi ngoài combat.

---

# 2. Attributes

## 2.1. Main Attributes

|Attribute|Viết tắt|Tác dụng trong combat|
|---|---|---|
|POTENCY|`POT`|Spell DMG + cường độ effect scaling.|
|SPIRIT|`SPI`|MP tối đa + MP Recovery scaling.|
|WISDOM|`WIS`|Yêu cầu **mở Spell Slot** và **Imprint** spell (`minWisdomToImprint` trên từng spell — xem _Game Overview_ / _Spells_). **Có thể** đóng góp vào `spell_potency` nếu Spell đó khai báo `weight_WIS > 0` — xem **Section 2.2.1**.|
|VITALITY|`VIT`|HP tối đa + Resistance scaling.|

> Người chơi bắt đầu với **10 điểm mỗi attribute**.
> 
> **Đã bỏ AGILITY** khỏi hệ Main Attribute. Không còn khái niệm dodge/miss — mọi đòn tấn công **luôn trúng 100%**. Slot Equipment `Boots` (trước đây focus AGI) cũng bị loại bỏ — xem _Game Overview — Section 2.1.6, 2.1.7_.

## 2.2. Sub Attributes

### 2.2.1. Potencies

Chỉ số phản ánh hiệu quả của các effect theo từng nguyên tố tương ứng.

**Multi-attribute scaling:** Từ bản cập nhật này, mỗi Spell **không còn scale cố định theo một mình `POT`**. Thay vào đó, mỗi Spell định nghĩa riêng một tập `scaling weight` cho từng Main Attribute (`POT`, `SPI`, `WIS`, `VIT`), lưu trong data của Spell đó (`SpellDefinition.scalingWeights`).

**Công thức tổng quát:**

```
spell_potency = Σ (weight_attr × attr_value)   với attr ∈ {POT, SPI, WIS, VIT}
```

|Ký hiệu|Mô tả|
|---|---|
|`weight_attr`|Tỷ trọng scaling của Spell theo từng Attribute — định nghĩa riêng cho từng Spell, mặc định là `{POT: 1.0}` (các Attribute khác = 0) nếu không khai báo, tương đương hành vi cũ.|
|`attr_value`|Giá trị hiện tại của Attribute đó (sau equipment/rune).|

`fire_potency`, `water_potency`, `ice_potency`, `lightning_potency` của một lượt cast được tính bằng `spell_potency` của Spell tương ứng với nguyên tố đó — không còn dùng chung 1 công thức `pot_scale × POT` cho mọi Spell.

**Ví dụ:** một Spell Nước thiên về hỗ trợ có thể khai báo `{SPI: 0.7, WIS: 0.3}` — dùng chủ yếu `SPI` nhưng vẫn hưởng lợi một phần từ `WIS`, khác với một Spell Nước thiên damage khai báo `{POT: 0.8, SPI: 0.2}`.

> Mục đích: cho phép các build khác nhau (không chỉ dồn `POT`) đều có Spell hiệu quả, đặc biệt giúp `WIS` — vốn trước đây chỉ dùng để mở Spell Slot / Imprint — có thêm giá trị trực tiếp trong combat tùy Spell được chọn.
> 
> Trang bị và Rune vẫn có thể điều chỉnh hệ số nhân theo từng nguyên tố (`fire_potency`, `water_potency`...) như cũ, áp dụng **sau** khi `spell_potency` đã được tính từ multi-attribute scaling.
> 
> Bộ `scaling weight` cụ thể cho từng Spell — xem tài liệu **Spell Design** riêng.

### 2.2.2. Resistances

Chỉ số phản ánh khả năng giảm sát thương nhận vào theo từng nguyên tố.

**Công thức VIT → Resistance:**

```
res_base = 10 × (VIT / 10)^EXPONENT_P
```

|Hằng số|Giá trị|Mô tả|
|---|---|---|
|`p`|`[TBD]` — Balancing|Exponent quyết định tốc độ tăng của Resistance so với VIT. `p = 1` → tuyến tính 1:1. `p > 1` → Resistance tăng **nhanh hơn** VIT (convex) — điểm VIT càng cao, mỗi điểm đóng góp càng nhiều Resistance.|

> Tại `VIT = 10` (khởi đầu), `res_base = 10 × (10/10)^p = 10` với **mọi giá trị p** — mốc khởi đầu luôn cố định bất kể p, nên có thể tune `p` tự do mà không lệch baseline.
> 
> Áp dụng cho cả 4 loại Resistance (Fire/Water/Ice/Lightning) — cùng công thức, cùng giá trị `p`:

```
fire_res       = res_base
water_res      = res_base
ice_res        = res_base
lightning_res  = res_base
```

**Bảng tham khảo (ví dụ minh họa với EXPONENT_P = 1.3, sẽ balancing lại sau):**

|VIT|res_base|
|---|---|
|10 (khởi đầu)|10.0|
|20|24.6|
|30|40.5|
|50|75.8|
|70|113.7|
|100|173.8|

> Trang bị và Rune có thể cộng thêm trực tiếp vào từng loại Resistance, độc lập với `res_base` từ VIT.

**Công thức áp dụng Resistance lên damage nhận vào:**

```
damage_reduction = R / (R + 90)
actual_damage    = incoming_damage × (1 - damage_reduction)
                 = incoming_damage × 90 / (R + 90)
```

Trong đó `R` là giá trị Resistance tương ứng với nguyên tố của đòn tấn công.

**Bảng tham khảo (Player với VIT = R gốc):**

|VIT|damage_reduction|actual_damage (từ 100 dmg)|
|---|---|---|
|10 (khởi đầu)|10.0%|90.0|
|20|18.2%|81.8|
|50|35.7%|64.3|
|90|50.0%|50.0|
|180|66.7%|33.3|
|∞|100% (tiệm cận)|0|

> Resistance không bao giờ đạt 100% — luôn có sát thương thực tế.  
> Trang bị và Rune có thể tăng từng loại Resistance độc lập với VIT.

### 2.2.3. MP Recovery

Lượng mana hồi **thêm** mỗi lượt ngoài `BASE_MP_RECOVERY`.

```
mp_recovery = 1.0 × SPI
```

---

# 3. HP, Armor & MP

## 3.1. HP

### Hurt Order

Thứ tự nhận sát thương được xác định bởi `hurt_order`:

- **HP** luôn có `hurt_order = 0`.
- Mỗi stack **Armor** nhận `hurt_order = current_max_hurt_order + 1` khi được apply.
- Sát thương luôn trừ vào đối tượng có `hurt_order` cao nhất trước, xuống đến HP cuối cùng.

### HP Scaling

HP tối đa scale theo `VIT` với quy luật **giảm dần**.

**Công thức:**

```
max_hp = BASE_HP_CAP × VIT / (VIT + BASE_HP_HALF_VIT)
```

| Hằng số            | Giá trị | Mô tả                                  |
| ------------------ | ------- | -------------------------------------- |
| `BASE_HP_CAP`      | `1000`  | HP tối đa tiệm cận (VIT → ∞)           |
| `BASE_HP_HALF_VIT` | `90`    | VIT tại đó `max_hp = HP_CAP / 2 = 500` |
	
> Tại VIT = 10 (khởi đầu): `max_hp = 1000 × 10 / (10 + 90) = 100`.

**Bảng tham khảo:**

| VIT           | max_hp | HP gain/điểm VIT |
| ------------- | ------ | ---------------- |
| 10 (khởi đầu) | 100    | —                |
| 15            | 143    | +8.5             |
| 20            | 182    | +7.7             |
| 30            | 250    | +6.8             |
| 50            | 357    | +5.4             |
| 90            | 500    | +3.6             |
| 180           | 667    | +1.9             |

## 3.2. Armor

**Armor** là một lớp HP tạm thời chặn sát thương trước khi trừ vào HP thật.

### Cấu trúc một Armor stack

```
armor_stack {
    value     : int,   // lượng HP của stack, xác định bởi spell/rune/item tạo ra
    duration  : int,   // số lượt còn lại
    hurt_order: int    // = current_max_hurt_order + 1 khi được apply
}
```

### Cơ chế hoạt động

- Mỗi lần Armor được apply → tạo một **stack mới** với `value` và `duration` riêng.
- Sát thương trừ vào stack có `hurt_order` cao nhất trước.
- Khi `value` về 0 → stack bị xóa, sát thương thừa tràn sang `hurt_order` kế tiếp.
- Khi `duration` về 0 → stack bị xóa bất kể còn bao nhiêu `value`.
- Duration giảm vào **Resolve Phase** của lượt bên sở hữu Armor (nhóm Tick — xem Section 5.1.1).

### Ví dụ

```
Tình huống:
  HP            (hurt_order=0)
  Armor Stack A (hurt_order=1, value=30, duration=2)
  Armor Stack B (hurt_order=2, value=20, duration=1)
 
Nhận 35 sát thương:
  → Stack B (hurt_order=2): nhận 20 dmg → value=0, bị xóa. 15 dmg tràn sang.
  → Stack A (hurt_order=1): nhận 15 dmg → value=15 còn lại.
  → HP: không bị ảnh hưởng.
```

## 3.3. MP

### Công thức

```
max_mp           = MP_COEFF × SPI
turn_mp_recovery = BASE_MP_RECOVERY + 1.0 × SPI
```

|Hằng số|Giá trị|Mô tả|
|---|---|---|
|`MP_COEFF`|`10`|Hệ số MP tối đa tuyến tính theo SPI|
|`BASE_MP_RECOVERY`|[Balancing]|Lượng MP hồi gốc mỗi lượt, không phụ thuộc SPI — quản lý qua Scriptable Object|

> MP scale **tuyến tính** theo SPI (khác với HP dùng diminishing returns).  
> Tại SPI = 10 (khởi đầu): `max_mp = 100`.

**Bảng tham khảo:**

|SPI|max_mp|turn_mp_recovery (BASE_MP_RECOVERY = 0)|
|---|---|---|
|10 (khởi đầu)|100|10|
|20|200|20|
|50|500|50|
|100|1000|100|

### Trạng thái đầu combat

- Người chơi bắt đầu combat với **MP đầy** (`current_mp = max_mp`).
- MP hồi lại vào **Resolve Phase** của mỗi lượt.

---

# 4. Enemy Attributes

Kẻ địch **không** dùng hệ Main Attribute của Player. Thay vào đó, mỗi enemy được định nghĩa trực tiếp bằng các giá trị sub attribute và danh sách spell.

### Cấu trúc một Enemy

```
enemy = {
    max_hp            : int,
 
    // Potencies
    fire_potency      : float,
    water_potency     : float,
    ice_potency       : float,
    lightning_potency : float,
 
    // Resistances (áp dụng cùng công thức R / (R + 90) như Player)
    fire_res          : float,
    water_res         : float,
    ice_res           : float,
    lightning_res     : float,
 
    // Behavior
    spells            : Spell[]   // danh sách spell được gắn sẵn
}
```

> **Số spell = số behavior** của enemy (Minion: 1–2, Elite: 2–3, Boss: 3–4).  
> Pattern hành vi (random, cycle, priority...) được định nghĩa trong tài liệu **Enemy Design** riêng.

---

# 5. Turn Logic & Structure

## 5.1. Turn Structure

Cấu trúc của 1 turn gồm 2 Phase:

```
Resolve Phase → Action Phase
```

> **Đã gộp Start Phase + End Phase cũ thành 1 Resolve Phase duy nhất**, chạy ở đầu mỗi lượt (trước Action Phase). Không còn End Phase riêng sau khi hành động.

### 5.1.1. Resolve Phase

Xử lý toàn bộ effect tại thời điểm resolve (MP, DoT, Regen, Frozen, Crystalize...), đồng thời giảm duration/cooldown của effect, Armor, Spell đang active — tất cả trong cùng 1 phase.

**Hồi MP:**

```
current_mp = min(current_mp + turn_mp_recovery, max_mp)
```

**Resolve Order:**

Chia làm 2 nhóm chạy tuần tự — nhóm **Tick** (đếm lùi duration/cooldown) chạy **trước**, nhóm **Value** (effect thực sự gây tác động) chạy **sau**:

|Nhóm|Thứ tự|Effect / Hành động|Lý do|
|---|---|---|---|
|Tick|1|Effect duration tick|Giảm duration effect có duration hữu hạn đang active (Regen, Distracted...). **Không áp dụng cho Frozen/Crystalize/Overdrive** — xem exception bên dưới.|
|Tick|2|Effect expiry check|Xóa các effect về 0 duration.|
|Tick|3|Armor duration tick|Giảm duration các Armor stack.|
|Tick|4|Armor expiry check|Xóa các Armor stack về 0 duration.|
|Tick|5|Spell cooldown tick|Giảm tất cả cooldown các spell đang cd.|
|Value|6|MP Recovery|Hồi mana.|
|Value|7|Frozen check|Nếu đang active → flag skip Action Phase, **tự giải NGAY** (one-shot).|
|Value|8|Crystalize check|Nếu đang active → bật flag miễn nhiễm damage + debuff cho lượt này, **tự giải NGAY**.|
|Value|9|Regen|Hồi HP trước DoT — tránh chết oan khi có cả hai cùng lúc.|
|Value|10|Burn (DoT)|Gây sát thương. Nếu Crystalize flag đang bật → bỏ qua.|
|Value|11|Các status còn lại|Enrage, Drenched, Chilled, Dazed, Fortified, Energized...|

> **Frozen / Crystalize / Overdrive là exception one-shot:** cả 3 **không** đi qua Effect duration tick (bước 1) — tự giải ngay tại bước check/dùng của chính nó. Lý do: nếu đi qua tick chung (chạy trước check), effect sẽ bị xóa trước khi kịp check active → skip Action Phase / miễn nhiễm sẽ không bao giờ trigger được dù combo đã đủ điều kiện.
> 
> **Hệ quả về duration cho Regen/Distracted/Armor:** vì Tick chạy trước Value, các effect này chỉ thực sự "sống" `duration - 1` chu kỳ full so với số khai báo — chu kỳ cuối bị tick xóa trước khi kịp trigger giá trị lần cuối (VD: Regen mất lần heal cuối, Distracted không kịp cộng mana cost cho Action Phase sắp tới, Armor hết tác dụng bảo vệ trước khi chủ nhân kịp hành động ở chính turn đó). Cooldown không bị ảnh hưởng vì Action Phase luôn nằm sau toàn bộ Resolve Phase.
> 
> **Lưu ý Crystalize:** Flag miễn nhiễm được bật ở bước 8 (trước Burn ở bước 10). Nếu Crystalize được _apply_ trong lượt này (chưa active vào đầu Resolve Phase) thì **không** có hiệu lực ngay — chỉ bảo vệ từ lượt sau.

**Combo check (Overdrive / Crystalize / Frozen / Detonates) — Event-driven:**

Không nằm trong Resolve Order cố định ở trên. Đây là một **rule phản ứng**: ngay sau khi bất kỳ Effect nào apply xong một buff/debuff mới lên 1 target — bất kể đang ở Action Phase hay Resolve Phase, lượt của Player hay Enemy — hệ thống lập tức kiểm tra target đó có đủ cặp điều kiện combo hay không và kích hoạt ngay nếu đủ.

- Áp dụng đồng nhất cho **cả 4 combo**, không riêng Detonates.
- Ví dụ: Player cast Fire rồi Lightning lên cùng 1 Enemy trong cùng 1 Action Phase → đủ Burn + Dazed ngay lúc đó → Detonates nổ **ngay lập tức**, không đợi tới Resolve Phase kế tiếp của Enemy đó.
- **Detonates là single-target**: chỉ gây damage lên chính target đang mang combo Burn + Dazed, không phải AoE lên toàn bộ phe địch.

### 5.1.2. Action Phase

Người chơi chọn các Spell để cast (hoặc Enemy thực hiện hành vi theo pattern).

- Người chơi có thể cast nhiều Spell trong một lượt miễn đủ MP.
- Nếu đang bị **Frozen**: toàn bộ Action Phase bị skip.
- Combo check (event-driven, xem 5.1.1) vẫn áp dụng trong Action Phase — mỗi lần 1 Spell apply xong buff/debuff lên target, hệ thống check combo ngay lập tức, không đợi tới Resolve Phase.

---

# 6. Effect Design

## 6.1. Buffs

| Effect         | Mô tả                                                                                                                                                      | Điều kiện apply                    | Duration       |
| -------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------- | -------------- |
| **Enrage**     | `all_potencies += enrage_modifire * all_potencies` (non-self-stackable)                                                                                    | Sử dụng phép Lửa lên bản thân      | ∞              |
| **Refreshing** | `mp_recovery += refreshing_modifier%` (non-self-stackable)                                                                                                 | Sử dụng phép Nước lên bản thân     | ∞              |
| **Fortified**  | `all_resistances += fortified_modifier * all_resistances` (non-self-stackable)                                                                             | Sử dụng phép Băng lên bản thân     | ∞              |
| **Energized**  | Các enemy bị tấn công bị sét đánh, nhận `energized_base_dmg + (energized_pot_scale * lightning_potency)` lightning damage (có thể bị giảm bởi resistances) | Sử dụng phép Sét lên bản thân      | ∞              |
| **Overdrive**  | `all_potencies += overdrive_modifier%`                                                                                                                     | Enrage + Energized (any order)     | This turn only |
| **Crystalize** | Miễn nhiễm damage và debuff (non-self-stackable)                                                                                                           | Refreshing + Fortified (any order) | Next turn only |
| **Regen**      | Hồi `regen_hp_percent * max_hp` vào Resolve Phase mỗi lượt (self-stackable)                                                                                | Spell / Item có apply Regen        | Depends        |

## 6.2. Debuffs

| Effect         | Mô tả                                                                                                                                                              | Điều kiện apply                  | Duration       |
| -------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------ | -------------------------------- | -------------- |
| **Burn**       | Nhận `burn_base_dmg + (burn_hp_percent * max_hp)` fire damage vào Resolve Phase mỗi lượt (có thể giảm bởi Resistance, non-stackable)                               | Chịu Damage Lửa                  | ∞              |
| **Drenched**   | `all_potencies -= drenched_modifier%` (non-self-stackable)                                                                                                         | Chịu Damage Nước                 | ∞              |
| **Chilled**    | `all_potencies -= %` (non-self-stackable)                                                                                                                          | Chịu Damage Băng                 | ∞              |
| **Dazed**      | `all_resistances -= dazed_modifier * all_resistances` (non-self-stackable)                                                                                         | Chịu Damage Sét                  | ∞              |
| **Detonates**  | Ngay lập tức nhận `detonates_hp_percent * max_hp` damage (không thể giảm bởi Resistance) — **single-target**, chỉ lên chính target đang mang combo, không phải AoE | Burn + Dazed (any order)         | Instant        |
| **Frozen**     | Vô hiệu hóa Action Phase (non-self-stackable)                                                                                                                      | Drenched + Chilled (any order)   | Next turn only |
| **Distracted** | `mana_cost += distracted_addition_percent%`                                                                                                                        | Spell / Item có apply Distracted | Depends        |

> **Thời điểm kích hoạt combo (Overdrive/Crystalize/Frozen/Detonates):** Event-driven, không phải bước cố định trong Resolve Order — kiểm tra ngay sau khi bất kỳ buff/debuff mới nào được apply lên 1 target, ở bất kỳ Phase nào. Chi tiết xem **Section 5.1.1**.

---

# 7. Ghi chú & Mục cần hoàn thiện

|Mục|Trạng thái|Ghi chú|
|---|---|---|
|BASE_MP_RECOVERY|⏳ Balancing|Quản lý qua Scriptable Object|
|`p` (Resistance exponent, Section 2.2.2)|⏳ Balancing|`res_base = 10 × (VIT/10)^p` — mốc VIT=10→Resistance=10 cố định bất kể p; p càng cao Resistance càng tăng tốc ở VIT lớn. Ví dụ minh họa hiện dùng p=1.3, cần playtest để chốt giá trị cuối.|
|Giá trị `+30%` của Overdrive và `-15%` của Chilled|⏳ Balancing|Giá trị tạm sau khi redefine từ AGI sang Potencies — xem Section 6.1, 6.2|
|`scalingWeights` cụ thể cho từng Spell|📄 Tài liệu riêng|Xem Section 2.2.1 — cần Spell Design chi tiết, tránh để 1 Attribute (thường là POT) luôn chiếm tỷ trọng áp đảo ở mọi Spell|
|Spell design chi tiết (bao gồm Rank I/II/III)|📄 Tài liệu riêng|—|
|Rune design chi tiết (bao gồm Rank I/II/III)|📄 Tài liệu riêng|—|
|Enemy Design (pattern, spell pool cụ thể)|📄 Tài liệu riêng|—|