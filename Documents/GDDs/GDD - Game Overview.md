---

---

---

**Game:** Wander 
**Author:** DukTofn 
**Last Updated:** 23/07/2026

---

## Mục lục

1. [Introduction](#1-introduction)
2. [Gameplay](#2-gameplay)
    - [2.1. Game Elements](#21-game-elements)
        - [2.1.1. Spells](#211-spells)
        - [2.1.2. Main Attributes](#212-main-attributes)
        - [2.1.3. Spell Slots](#213-spell-slots)
        - [2.1.4. MP](#214-mp)
        - [2.1.5. HP](#215-hp)
        - [2.1.6. Equipment](#216-equipment)
        - [2.1.7. Equipment Slots](#217-equipment-slots)
        - [2.1.8. Runes](#218-runes)
        - [2.1.9. Rune Sockets](#219-rune-sockets)
        - [2.1.10. Magic Shop](#2110-magic-shop)
        - [2.1.11. Enemies](#2111-enemies)
        - [2.1.12. Nodes](#2112-nodes)
    - [2.2. Core Mechanics](#22-core-mechanics)
        - [2.2.1. Elemental Effects](#221-elemental-effects)
        - [2.2.2. Element Interactions](#222-element-interactions)
        - [2.2.3. Turn Logic](#223-turn-logic)
    - [2.3. Progression](#23-progression)
        - [2.3.1. Level Structure](#231-level-structure)
        - [2.3.2. Combat Win/Lose Condition](#232-combat-winlose-condition)
3. [Theme](#3-theme)
    - [3.1. Visual Style](#31-visual-style)
    - [3.2. Sound Direction](#32-sound-direction)

---

# 1. Introduction

**Name:** Wandering Wanderer.  
**Genre:** Turn-Based, Strategy, Mid-core.  
**Engine:** Unity.

**Game Pitch:** Wandering Wanderer là một game mid-core với hệ thống combat theo lượt. Người chơi nhập vai vào một pháp sư trên chuyến hành trình đi về quê hương của mình ở một vùng đất xa xăm. Trong suốt cuộc hành trình, vị pháp sư phải chiến đấu với nhiều kẻ địch, thu thập các trang bị để tăng sức mạnh cho bản thân, học các phép thuật mới để phục vụ quá trình chiến đấu.

---

# 2. Gameplay

## 2.1. Game Elements

### 2.1.1. Spells

- Là công cụ chính để người chơi vượt qua các thử thách trong game.
- Khi sử dụng sẽ tiêu tốn MP.
- Chia ra làm 4 nguyên tố chính: Lửa, Nước, Băng, Sét.
- Mỗi loại phép gây ra buff và debuff riêng biệt.
- **Scaling đa chỉ số:** Mỗi spell có thể scale theo **nhiều hơn 1 Main Attribute** cùng lúc, với tỷ trọng (`scaling weight`) riêng cho từng chỉ số — định nghĩa trực tiếp trong data của spell đó. Ví dụ một spell Nước có thể scale chính theo `SPI` và một phần theo `WIS`. Mục đích: cho phép các chỉ số vốn không trực tiếp ra damage (như `WIS`) vẫn có giá trị trong combat tùy build, tránh tình trạng dump-stat. Chi tiết công thức xem tại _Combat Design — Section 2.2.1_.
- Spell chia làm **3 Rank** theo mức độ sức mạnh:

|Rank|Mô tả|
|---|---|
|Rank I|Spell cơ bản — hiệu ứng đơn giản, damage/effect thấp|
|Rank II|Spell nâng cao — hiệu ứng mạnh hơn|
|Rank III|Spell hiếm — hiệu ứng mạnh kinh hoàng, tốn MP cũng kinh hoàng|

### 2.1.2. Main Attributes

Chỉ số của người chơi, ảnh hưởng trực tiếp đến quá trình chiến đấu. Có 5 loại chỉ số:

|Attribute|Viết tắt|Tác dụng|
|---|---|---|
|POTENCY|`POT`|Tăng sát thương và cường độ hiệu ứng của các phép.|
|SPIRIT|`SPI`|Tăng MP tối đa và lượng MP hồi lại mỗi lượt.|
|WISDOM|`WIS`|Đáp ứng **hai** nhóm yêu cầu liên quan phép: (1) **tự động mở khóa Spell Slot** khi đủ ngưỡng (xem `WisdomSlotConfig`) — không cần thao tác hay tốn Gold; (2) ngưỡng **WIS tối thiểu để Imprint** từng spell — mỗi spell có `minWisdomToImprint` riêng trong data (xem _Spells_).|
|VITALITY|`VIT`|Tăng HP tối đa và các chỉ số kháng nguyên tố.|

> Người chơi bắt đầu với **10 điểm mỗi attribute**.

### 2.1.3. Spell Slots

- Những ô để gắn phép, chỉ các phép được gắn trong ô mới có thể sử dụng trong combat.
- Người chơi bắt đầu với 1 ô phép. Các ô tiếp theo **tự động mở khóa** ngay khi `WIS` hiện tại đạt ngưỡng tương ứng (`WisdomSlotConfig`) — không cần vào Shop, không tốn Gold, không cần thao tác gì thêm. Tối đa 5 ô khi max `WIS`.
- Gắn phép vào ô thông qua `Imprint Spells`, gỡ ra thông qua `Forget Spells` (chỉ khả dụng ngoài combat).
- **WIS khi Imprint:** mỗi spell có **ngưỡng WIS tối thiểu** (`minWisdomToImprint`) độc lập với việc mở slot. Chỉ khi `WIS` hiện tại của nhân vật (sau equipment/rune) **≥** ngưỡng của spell đó thì mới được Imprint vào một ô đã mở. `Forget` không đòi hỏi WIS.

### 2.1.4. MP

- Là tài nguyên tiêu hao để cast spell.
- Người chơi bắt đầu trận chiến với đầy MP. Mỗi lượt được hồi lại một lượng nhất định.
- MP tối đa và lượng hồi mỗi lượt tăng **tuyến tính** theo `SPI`.

### 2.1.5. HP

- Là tài nguyên sống còn của người chơi.
- Người chơi bắt đầu một run với đầy HP. Chỉ hồi lại khi dùng Potion hoặc Rest tại Rest Node.
- HP tối đa scale theo `VIT` với quy luật **giảm dần** — điểm `VIT` đầu tiên cho nhiều HP hơn các điểm sau.

### 2.1.6. Equipment

Trang bị của người chơi, điều chỉnh các Main Attributes (`POT`, `SPI`, `WIS`, `VIT`) và cung cấp passive. Trang bị chia ra 4 loại:

|Loại|Tên|Focus|
|---|---|---|
|Trượng|Staff|`POT`|
|Nhẫn|Ring|`SPI`|
|Sách|Book|`WIS`|
|Y phục|Garb|`VIT`|

Trang bị chia làm **3 Rank** theo mức độ sức mạnh:

|Rank|Mô tả|
|---|---|
|Rank I|Cung cấp 1 chỉ số|
|Rank II|Cung cấp 2 chỉ số (hoặc ít hơn nhưng giá trị cao hơn)|
|Rank III|Cung cấp 3 chỉ số (hoặc ít hơn nhưng giá trị cao hơn)|

### 2.1.7. Equipment Slots

Các ô để gắn trang bị, tương ứng 1-1 với 4 loại trang bị:

- **Staff Slot** — Ô Trượng
- **Ring Slot** — Ô Nhẫn
- **Book Slot** — Ô Sách
- **Garb Slot** — Ô Y phục

### 2.1.8. Runes

- Là những viên đá có thể khảm vào bản thân thông qua `Embed`.
- Không tăng Main Attributes, nhưng mỗi viên cung cấp passive riêng biệt.
- `Embed` và `Purge` (tháo Rune) đều **miễn phí**, thực hiện được **bất cứ lúc nào ngoài combat** — không cần vào Node nào cụ thể.
- Có thể nhận Rune từ Chest, mua trong Magic Shop, hoặc drop từ Combat.
- Rune chia làm **3 Rank** theo mức độ sức mạnh của passive:

|Rank|Mô tả|
|---|---|
|Rank I|Passive đơn giản, hiệu quả thấp|
|Rank II|Passive mạnh hơn hoặc có điều kiện kích hoạt|
|Rank III|Passive mạnh, thường ảnh hưởng đến nhiều cơ chế cùng lúc|

### 2.1.9. Rune Sockets

- Các ô để khảm Runes.
- Người chơi có sẵn **cả 4 Sockets ngay từ đầu run** — không cần mở khóa hay mua thêm.

### 2.1.10. Magic Shop

- Mỗi shop có **15 mặt hàng** chia thành **3 gian hàng**, mỗi gian có **5 mặt hàng** của một loại: Equipment, Spell, Rune.
- Rank của mặt hàng phụ thuộc vào tiến trình game (Arc sau có xu hướng xuất hiện Rank cao hơn).
- Chỉ bán vật phẩm — **không cung cấp dịch vụ nào khác** (Enlighten, Embed, Purge, mua Rune Socket đã được loại bỏ; các cơ chế tương ứng giờ tự động hoặc miễn phí — xem 2.1.3, 2.1.8, 2.1.9).
- Giá cố định theo Rank, không phân biệt loại mặt hàng. Chi tiết xem tại _Progression Design — Section 4_.

### 2.1.11. Enemies

Kẻ địch chia làm 3 loại theo độ phức tạp hành vi:

|Loại|Số hành vi (Spell)|Ghi chú|
|---|---|---|
|Minion|1 – 2|Quái thường|
|Elite|2 – 3|Quái tinh anh|
|Boss|3 – 4|Quái trùm|

Mỗi enemy được định nghĩa trực tiếp bằng HP, Potencies, Resistances và danh sách spell được gắn sẵn — không dùng hệ Main Attribute của Player. Chi tiết xem tại _Combat Design — Section 4_.

### 2.1.12. Nodes

Đơn vị nhỏ nhất trên bản đồ. Các loại:

- **Combat Node:** Vào combat. Chia 3 loại nhỏ tương ứng với loại kẻ thù (Normal / Elite / Boss).
- **Camp Node:** Hồi phục toàn bộ HP/MP và cộng thêm 1 điểm vào Main Attribute tùy chọn.
- **Merchant Node:** Mở Magic Shop (mua bán, không có Camp).
- **Event Node:** Sự kiện ngẫu nhiên — có thể tích cực, tiêu cực hoặc đánh đổi.
- **Town Node:** Node hiếm, gộp cả chức năng của **Camp** và **Merchant** (hồi full trạng thái, +1 Main Attribute, và mở Magic Shop) trong cùng một node. Chi tiết tần suất và ràng buộc vị trí xem tại _Progression Design — Section 2_.

---

## 2.2. Core Mechanics

### 2.2.1. Elemental Effects

Là các buff/debuff gây ra bởi các phép nguyên tố.

- Dùng phép lên **bản thân/đồng minh** → apply **buff**.
- Dùng phép lên **kẻ địch** → apply **debuff**.

|Element|**Fire**|**Water**|**Ice**|**Lightning**|
|---|---|---|---|---|
|**Buff (+)**|Enrage (DMG+)|Refreshing (Mana Recover)|Fortified (DEF+)|Energized (DMG+)|
|**Debuff (−)**|Burn (DoT)|Drenched (DMG−)|Chilled (DMG−)|Dazed (DEF−)|

**Các effect đặc biệt** (kích hoạt khi đồng thời có 2 effect nhất định):

|Effect|Điều kiện|Mô tả|
|---|---|---|
|**Detonates**|Burn + Dazed|Gây Damage Burst lên tất cả kẻ địch|
|**Frozen**|Drenched + Chilled|Vô hiệu hóa mục tiêu trong lượt tiếp theo|
|**Overdrive**|Enrage + Energized|Lượt tiếp theo: gây sát thương tăng mạnh (cộng dồn thêm `all_potencies`)|
|**Crystalize**|Refreshing + Fortified|Lượt tiếp theo: miễn nhiễm sát thương và debuff|

**Các effect độc lập khác:**

- **Regen:** Hồi HP vào Start Phase mỗi lượt.
- **Armor:** Chặn một lượng sát thương nhất định trước khi trừ vào HP.
- **Distracted:** Tăng mana cost của spell trong một số lượt nhất định.

### 2.2.2. Element Interactions

Quy ước tương khắc:

```
Nước > Lửa > Băng > Sét > Nước
">" = Khắc
```

|Tình huống|Kết quả|
|---|---|
|Effect B đang active, apply Effect A mà B > A|Effect A bị giải, B giữ nguyên.|
|Effect B đang active, apply Effect A mà A > B|Effect B bị giải, apply Effect A.|
|Effect A đang active, apply buff/debuff cùng nguyên tố A|Cả hai bị giải trừ (Neutralize).|

### 2.2.3. Turn Logic

Bắt đầu game với Player Turn → Enemy Turn → lặp lại luân phiên.

**Cấu trúc Turn:**

```
Resolve Phase → Action Phase
```

> **Đã gộp Start Phase + End Phase cũ thành 1 Resolve Phase duy nhất**, chạy ở đầu mỗi lượt (trước Action Phase). Không còn End Phase riêng sau khi hành động.

- **Resolve Phase:** Hồi MP, xử lý toàn bộ effect tại thời điểm resolve (DoT, Regen, Frozen, Crystalize...), **và** giảm duration/cooldown của các effect, Armor, Spell đang active — tất cả trong cùng 1 phase.
- **Action Phase:** Người chơi chọn Spell để cast (hoặc Enemy thực hiện hành vi). Bị skip hoàn toàn nếu đang Frozen.

Chi tiết resolve order bên trong Resolve Phase (thứ tự giữa tick duration và các effect như Burn/Regen/Frozen/Crystalize) xem tại _GDD — Combat Design, Section 5_.

---

## 2.3. Progression

### 2.3.1. Level Structure

Trong một run, người chơi đi qua 3 Arc. Mỗi Arc gồm nhiều Node nối với nhau thành dạng đồ thị có hướng. Người chơi chỉ có thể di chuyển sang Node kề phía trước và **không thể quay lại Node đã đi qua**. Bản đồ áp dụng cơ chế **Fog of War**: chỉ các Node đã discover mới hiện rõ hoàn toàn, các Node chưa discover chỉ nhìn thấy trước tối đa 2 lớp (layer), phần còn lại bị sương mù che khuất. Chi tiết xem tại _GDD — Progression Design_.

### 2.3.2. Combat Win/Lose Condition

- **WIN:** Tiêu diệt toàn bộ kẻ địch trong combat.
- **LOSE:** HP của Player về 0.

---

# 3. Theme

## 3.1. Visual Style

[TBD]

## 3.2. Sound Direction

[TBD]