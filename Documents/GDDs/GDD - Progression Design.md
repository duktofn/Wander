___
**Game:** Wandering Wanderer
**Author:** DukTofn
**Last Updated:** 05/04/2026
___

## Mục lục
1. [Document Direction](#1-document-direction)
2. [Map Structure](#2-map-structure)
   - [2.1. Tổng quan](#21-tổng-quan)
   - [2.2. Cấu trúc mỗi Arc](#22-cấu-trúc-mỗi-arc)
   - [2.3. Phân bổ Node trong Arc](#23-phân-bổ-node-trong-arc)
   - [2.4. Branching](#24-branching)
3. [Item Ranks](#3-item-ranks)
   - [3.1. Equipment](#31-equipment)
   - [3.2. Spell](#32-spell)
   - [3.3. Rune](#33-rune)
4. [Magic Shop](#4-magic-shop)
   - [4.1. Inventory](#41-inventory)
   - [4.2. Bảng giá cố định](#42-bảng-giá-cố-định)
   - [4.3. Dịch vụ](#43-dịch-vụ)
5. [Combat Rewards](#5-combat-rewards)
   - [5.1. Gold Reward](#51-gold-reward)
   - [5.2. Item Reward](#52-item-reward)
6. [Event Details](#6-event-details)
   - [6.1. Cơ chế](#61-cơ-chế)
   - [6.2. Event Pool](#62-event-pool)
7. [Ghi chú & Mục cần hoàn thiện](#7-ghi-chú--mục-cần-hoàn-thiện)

---

# 1. Document Direction

Tài liệu này mô tả cấu trúc tiến trình của một run, bao gồm cách bản đồ được tạo ra, nội dung chi tiết của từng loại Node, cơ chế Magic Shop, phần thưởng sau combat và các Event ngẫu nhiên.

Không bao gồm cơ chế combat và hệ thống attribute — xem tại _GDD — Combat Design_.

---

# 2. Map Structure

## 2.1. Tổng quan

Một run gồm **3 Arc** đi theo thứ tự tuyến tính. Mỗi Arc là một đồ thị có hướng gồm nhiều Node nối với nhau. Người chơi bắt đầu từ Node đầu tiên và chỉ có thể di chuyển sang các Node kề phía trước — **không thể quay lại**.

```
[Arc 1] → [Arc 2] → [Arc 3]
```

## 2.2. Cấu trúc mỗi Arc

Mỗi Arc có tổng số Node **cố định** trên map nhưng **layout ngẫu nhiên** mỗi run. Người chơi chỉ đi qua một tập con các Node đó tùy đường chọn — phần lớn Node trên map sẽ không được visit trong một run, tạo replayability qua nhiều run.

||Arc 1|Arc 2|Arc 3|
|---|---|---|---|
|**Tổng số Node trên map**|50|75|100|
|**Node cuối (bắt buộc)**|Boss|Boss|Boss|
|**Số Node tối thiểu để đến Node cuối**|15|20|25|
|**Số Elite Node tối thiểu trên đường**|1|2|2|
|**Số Rest Node tối thiểu trên đường**|2|2|3|
|**Số Shop Node tối thiểu trên đường**|2|2|3|
|**Số Event Node tối thiểu trên đường**|2|3|3|
|**Số Optional Boss trên toàn map**|1|2|3|

> Số Node tối thiểu áp dụng cho **đường ngắn nhất có thể** — thuật toán sinh map phải đảm bảo mọi đường dẫn đến Boss cuối đều thoả mãn ràng buộc này.

## 2.3. Phân bổ Node trong Arc

Số Node mỗi loại cố định, layout ngẫu nhiên mỗi run.---

## 2.4. Branching

- Mỗi Node có thể nối tới **1–3 Node** ở hàng kế tiếp.
- Người chơi thấy loại Node phía trước trước khi chọn đường đi.
- Chi tiết thuật toán sinh layout xem tại tài liệu **Tech Design — Map Generation**.
- Không thể đi tới một Node cùng loại trong 2 Node tiếp theo trừ Combat Node của Minions.

---

# 3. Item Ranks

Tất cả các loại vật phẩm có thể nhận được (Equipment, Spell, Rune) đều chia làm **3 Rank**. Rank cao hơn đồng nghĩa với sức mạnh cao hơn và độ hiếm cao hơn.

## 3.1. Equipment

|Rank|Số chỉ số cung cấp|
|---|---|
|Rank I|1 chỉ số|
|Rank II|2 chỉ số (hoặc ít hơn nhưng giá trị cao hơn)|
|Rank III|3 chỉ số (hoặc ít hơn nhưng giá trị cao hơn)|

## 3.2. Spell

|Rank|Mô tả|
|---|---|
|Rank I|Spell cơ bản — hiệu ứng đơn giản, damage/effect thấp|
|Rank II|Spell nâng cao — hiệu ứng mạnh hơn hoặc có thêm điều kiện kích hoạt|
|Rank III|Spell hiếm — damage/effect cao nhất, thường có cơ chế đặc biệt|

## 3.3. Rune

|Rank|Mô tả|
|---|---|
|Rank I|Passive đơn giản, hiệu quả thấp|
|Rank II|Passive mạnh hơn hoặc có điều kiện kích hoạt|
|Rank III|Passive mạnh, thường ảnh hưởng đến nhiều cơ chế cùng lúc|

---

# 4. Magic Shop

## 4.1. Inventory

Mỗi lần vào Shop, hệ thống tạo ra **15 mặt hàng** chia đều thành **3 gian hàng**, mỗi gian có **5 mặt hàng** của một loại:

|Gian hàng|Nội dung|
|---|---|
|Equipment|5 Equipment ngẫu nhiên|
|Spell|5 Spell ngẫu nhiên|
|Rune|5 Rune ngẫu nhiên|

**Tỷ lệ Rank của từng loại khi xuất hiện trong Shop (theo Arc):**

|Rank|Arc 1|Arc 2|Arc 3|
|---|---|---|---|
|Rank I|[TBD]%|[TBD]%|[TBD]%|
|Rank II|[TBD]%|[TBD]%|[TBD]%|
|Rank III|[TBD]%|[TBD]%|[TBD]%|

> Tỷ lệ Rank áp dụng giống nhau cho cả Equipment, Spell và Rune trong Shop. Rank III nên hiếm ở Arc 1 và phổ biến hơn ở Arc 3.

## 4.2. Bảng giá cố định

Tất cả giá tính bằng **Gold**, cố định theo Rank (không phân biệt loại mặt hàng).

|Rank|Giá|
|---|---|
|Rank I|[TBD] G|
|Rank II|[TBD] G|
|Rank III|[TBD] G|

## 4.3. Dịch vụ

Ngoài mua đồ, Shop còn cung cấp các dịch vụ sau:

|Dịch vụ|Mô tả|Giá|
|---|---|---|
|**Enlighten**|Mở thêm 1 Spell Slot — yêu cầu đủ `WIS` theo **ngưỡng mở slot** (`WisdomSlotConfig`, slot tiếp theo). *Khác* với ngưỡng WIS để **Imprint** từng spell (`minWisdomToImprint` trên `SpellDefinition`).|[TBD] G|
|**Embed**|Khảm 1 Rune vào Rune Socket|[TBD] G|
|**Purge**|Tháo 1 Rune khỏi Socket|[TBD] G|
|**Rune Socket**|Mở thêm 1 Rune Socket (tối đa 4)|Xem bảng bên dưới|

**Giá mua Rune Socket (tăng lũy tiến theo số Socket đã có):**

|Socket thứ|Giá|
|---|---|
|1|[TBD] G|
|2|[TBD] G|
|3|[TBD] G|
|4|[TBD] G|

> Giá Rune Socket tăng lũy tiến để tạo quyết định đánh đổi — mở Socket hay mua đồ.

---

# 5. Combat Rewards

Sau khi thắng một trận combat, người chơi nhận được:

1. **Gold** (cố định theo loại kẻ địch)
2. **Chọn 1 trong 3 phần thưởng** — hệ thống offer đúng 1 Equipment + 1 Spell + 1 Rune, người chơi chọn 1

## 5.1. Gold Reward

|Loại combat|Gold nhận được|
|---|---|
|Minion Node|[TBD] G|
|Elite Node|[TBD] G|
|Boss Node (Optional)|[TBD] G|
|Boss Node (Bắt buộc, Node cuối Arc)|[TBD] G|

## 5.2. Item Reward

Hệ thống offer **1 Equipment + 1 Spell + 1 Rune**, người chơi chọn **đúng 1** trong 3.

**Tỷ lệ Rank của reward (theo loại combat và Arc):**

_Minion & Elite Node:_

|Rank|Arc 1|Arc 2|Arc 3|
|---|---|---|---|
|Rank I|55%|40%|25%|
|Rank II|35%|40%|40%|
|Rank III|10%|20%|35%|

_Boss Node (Optional & Bắt buộc):_

|Rank|Arc 1|Arc 2|Arc 3|
|---|---|---|---|
|Rank I|20%|10%|0%|
|Rank II|50%|40%|30%|
|Rank III|30%|50%|70%|

> Boss Node luôn drop Rank cao hơn đáng kể so với cùng Arc để xứng đáng với rủi ro.

---

# 6. Event Details

## 6.1. Cơ chế

Khi vào Event Node, hệ thống chọn **ngẫu nhiên 1 Event** từ pool. Event không lặp lại trong cùng một Arc (nếu pool đủ lớn).

**Phân loại Event:**

|Ký hiệu|Loại|Mô tả|
|---|---|---|
|(+)|Tích cực|Chỉ cho lợi ích|
|(=)|Đánh đổi|Có cả lợi và hại, người chơi quyết định|
|(−)|Tiêu cực|Chỉ gây bất lợi|

> Khuyến nghị tỷ lệ phân bổ: ~50% tích cực / ~30% đánh đổi / ~20% tiêu cực để tránh Event Node cảm giác quá an toàn.

## 6.2. Event Pool

|Event|Loại|Mô tả|Tỷ lệ|
|---|---|---|---|
|**Windfall**|(+)|Nhận một lượng Gold ngẫu nhiên (`[TBD]–[TBD] G`).|[TBD]%|
|**Ancient Shrine**|(+)|Nhận +1 điểm Main Attribute tùy chọn.|[TBD]%|
|**Wandering Merchant**|(+)|Mở một mini-shop với 3 mặt hàng ngẫu nhiên, giá giảm 25%.|[TBD]%|
|**Hidden Cache**|(+)|Nhận miễn phí 1 Rune ngẫu nhiên.|[TBD]%|
|**Cursed Altar**|(=)|Mất `[TBD]%` HP hiện tại để nhận +3 điểm Main Attribute tùy chọn.|[TBD]%|
|**Lost Devil**|(=)|Chiến đấu với 1 Elite — nếu thắng, reward chắc chắn offer Spell Rank III.|[TBD]%|
|**Force Trade**|(=)|Trong trận combat ngay tiếp theo: `all_res -= 15%`, `all_potencies += 15%`.|[TBD]%|
|**Ambush**|(−)|Phải chiến đấu ngay với 1 Minion — không nhận Combat Rewards nếu thắng.|[TBD]%|
|**Fading Curse**|(−)|Trong trận combat ngay tiếp theo: `all_res -= 10%`.|[TBD]%|
|**Thief Gang**|(−)|Mất 25% Gold hiện có.|[TBD]%|

> **Lưu ý — Event ảnh hưởng combat:** _Force Trade_ và _Fading Curse_ áp dụng modifier **chỉ cho trận combat ngay tiếp theo** (không kéo dài nhiều Node), tránh cần định nghĩa thêm persistent modifier system giữa các Node.

---

# 7. Ghi chú & Mục cần hoàn thiện

|Mục|Trạng thái|Ghi chú|
|---|---|---|
|Tỷ lệ phân bổ Node mỗi loại trong Arc|⏳ Balancing|Cần playtesting để cân thời lượng và độ khó một run|
|Thuật toán sinh layout Map|📄 Tech Design|Cần đảm bảo ràng buộc số Node tối thiểu mọi đường|
|Giá tất cả mặt hàng và dịch vụ Shop|⏳ Balancing|Cần Gold economy cơ bản trước|
|Gold Reward từng loại combat|⏳ Balancing|Liên quan trực tiếp đến Gold economy|
|Tỷ lệ Rank trong Shop và Reward|⏳ Balancing|Liên quan đến power curve của run|
|Tỷ lệ và giá trị cụ thể của từng Event|⏳ Balancing|—|
|Rank system của Spell và Rune (nội dung cụ thể)|📄 Tài liệu riêng|Spell Design / Rune Design|
|Bổ sung Event pool nếu cần|🔜 Mở rộng|Hiện có 10 Event|
|Enemy Design|📄 Tài liệu riêng|—|