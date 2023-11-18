-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'IDSkill
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '200005' bên dưới thành ID tương ứng
local RuongBiKip120 = Scripts[200109]
--SKILL 120X
local IDSkill = {
    [8519] = {
        RequireFaction = 1,
        RequireLevel = 120,
        SkillID = 10022,
        Name = "Kỹ Năng Đại Thừa Như Lai Chú",
        Number = 1,
        Series = -1,
        bind = 0,
    },
    [8520] = {
        RequireFaction = 2,
        RequireLevel = 120,
        SkillID = 10052,
        Name = "Kỹ Năng Đảo Hư Thiên",
        Number = 1,
        Series = -1,
        bind = 0,
    },
    [8521] = {
        RequireFaction = 4,
        RequireLevel = 120,
        SkillID = 10098,
        Name = "Kỹ Năng Hấp Tinh Yểm",
        Number = 1,
        Series = -1,
        bind = 0,
    },
    [8522] = {
        RequireFaction = 3,
        RequireLevel = 120,
        SkillID = 10157,
        Name = "Kỹ Năng Mê Ảnh Tung",
        Number = 1,
        Series = -1,
        bind = 0,
    },
    [8523] = {
        RequireFaction = 5,
        RequireLevel = 120,
        SkillID = 10176,
        Name = "Kỹ Năng Bế Nguyệt Phất Trần",
        Number = 1,
        Series = -1,
        bind = 0,
    },
    [8524] = {
        RequireFaction = 6,
        RequireLevel = 120,
        SkillID = 10085,
        Name = "Kỹ Năng Ngự Tuyết Ẩn",
        Number = 1,
        Series = -1,
        bind = 0,
    },
    [8525] = {
        RequireFaction = 7,
        RequireLevel = 120,
        SkillID = 10083,
        Name = "Kỹ Năng Hỗn Thiên Khí Công",
        Number = 1,
        Series = -1,
        bind = 0,
    },
    [8526] = {
        RequireFaction = 8,
        RequireLevel = 120,
        SkillID = 10080,
        Name = "Kỹ Năng Ma Âm Phệ Phách",
        Number = 1,
        Series = -1,
        bind = 0,
    },
    [8527] = {
        RequireFaction = 9,
        RequireLevel = 120,
        SkillID = 10267,
        Name = "Kỹ Năng Xuất Ứ Bất Nhiễm",
        Number = 1,
        Series = -1,
        bind = 0,
    },
    [8528] = {
        RequireFaction = 10,
        RequireLevel = 120,
        SkillID = 10296,
        Name = "Kỹ Năng lưỡng Nghi Chân Khí",
        Number = 1,
        Series = -1,
        bind = 0,
    },
    [8529] = {
        RequireFaction = 11,
        RequireLevel = 120,
        SkillID = 11377,
        Name = "Kỹ Năng Hạo Nhiên Chi Khí",
        Number = 1,
        Series = -1,
        bind = 0,
    },
    [8530] = {
        RequireFaction = 11,
        RequireLevel = 120,
        SkillID = 11371,
        Name = "Kỹ Năng Tử Hà Kiếm Khí",
        Number = 1,
        Series = -1,
        bind = 0,
    },
    [8568] = {
        RequireFaction = 12,
        RequireLevel = 120,
        SkillID = 11371,
        Name = "Kỹ Năng Không tuyệt tâm pháp",
        Number = 1,
        Series = -1,
        bind = 0,
    },
    [8569] = {
        RequireFaction = 12,
        RequireLevel = 120,
        SkillID = 11371,
        Name = "Kỹ Năng thánh hỏa thần công",
        Number = 1,
        Series = -1,
        bind = 0,
    },
    ----------------------------------------------------------------
    [8574] = {
        RequireFaction = 16,
        RequireLevel = 120,
        SkillID = 1513,
        Name = "Kỹ Năng Càn Nguyên Trấn Hải Quyết",
        Number = 1,
        Series = -1,
        bind = 0,
    },
    [8575] = {
        RequireFaction = 16,
        RequireLevel = 120,
        SkillID = 1516,
        Name = "Kỹ Năng Binh Phong Trực Chỉ",
        Number = 1,
        Series = -1,
        bind = 0,
    },
    [8576] = {
        RequireFaction = 16,
        RequireLevel = 120,
        SkillID = 1569,
        Name = "Kỹ Năng Lôi Tự Kim Bình",
        Number = 1,
        Series = -1,
        bind = 0,
    },
    [8577] = {
        RequireFaction = 16,
        RequireLevel = 120,
        SkillID = 1566,
        Name = "Kỹ Năng Ngạo Khí Hàn Sương Quyết",
        Number = 1,
        Series = -1,
        bind = 0,
    },
    ------------------Tiêu dao----------------------------------------------
    [34607] = {
        RequireFaction = 15,
        RequireLevel = 120,
        SkillID = 1413,
        Name = "Kỹ Năng Phục Nhật Xuất Vân",
        Number = 1,
        Series = -1,
        bind = 0,
    },
    [34608] = {
        RequireFaction = 15,
        RequireLevel = 120,
        SkillID = 1417,
        Name = "Kỹ Năng Hỗn Nhật Khí Quyết",
        Number = 1,
        Series = -1,
        bind = 0,
    },
    [34609] = {
        RequireFaction = 15,
        RequireLevel = 120,
        SkillID = 1465,
        Name = "Kỹ Năng Hỏa Hải Vô Nhai",
        Number = 1,
        Series = -1,
        bind = 0,
    },
    [34610] = {
        RequireFaction = 15,
        RequireLevel = 120,
        SkillID = 1467,
        Name = "Kỹ Năng Phần Phách Tru Tâm",
        Number = 1,
        Series = -1,
        bind = 0,
    },
}

-- ****************************************************** --
--	Hàm này được gọi khi người chơi ấn sử dụng vật phẩm, kiểm tra điều kiện có được dùng hay không
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
--	Return: True nếu thỏa mãn điều kiện có thể sử dụng, False nếu không thỏa mãn
-- ****************************************************** --
function RuongBiKip120:OnPreCheckCondition(scene, item, player, otherParams)
    -- ************************** --
    --player:AddNotification("Enter -> RuongBiKip120:OnPreCheckCondition")--
    -- ************************** --
    return true
    -- ************************** --
end

-- ****************************************************** --
--	Hàm này được gọi để thực thi Logic khi người sử dụng vật phẩm, sau khi đã thỏa mãn hàm kiểm tra điều kiện
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
-- ****************************************************** --
function RuongBiKip120:OnUse(scene, item, player, otherParams)
    -- ************************** --
    -- ************************** --
    local dialog = GUI.CreateItemDialog()
    if Player.HasFreeBagSpaces(player, 5) == false then
        player:AddNotification(
            "Túi của ngươi đã đầy, hãy sắp xếp <color=green>5 ô trống</color> trong túi để nhận kỹ năng 120x!")
        return
    end
    dialog:AddText(
    "Dùng được kỹ năng cực phẩm. Hãy chọn kỹ năng mình cần.<br><color=#53f9e8>(Nhận Kỹ năng sẽ tự động khóa)</color>")
    for key, value in pairs(IDSkill) do
        dialog:AddSelection(key, value.Name)
    end
    dialog:AddSelection(5, "Để ta suy nghĩ đã")
    dialog:Show(item, player)
    -- ************************** --
end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi ấn vào một trong số các chức năng cung cấp bởi vật phẩm thông qua Item Dialog
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
--		selectionID: number - ID chức năng
-- ****************************************************** --
function RuongBiKip120:OnSelection(scene, item, player, selectionID, otherParams)
    -- ************************** --
    --player:AddNotification("Enter -> RuongBiKip120:OnSelection, selectionID = " .. selectionID)--

    local dialog = GUI.CreateItemDialog()
    if Player.HasFreeBagSpaces(player, 5) == false then
        player:AddNotification(
            "Túi của ngươi đã đầy, hãy sắp xếp <color=green>5 ô trống</color> trong túi để nhận kỹ năng 120x!")
        return
    end
    for key, value in pairs(IDSkill) do
        if selectionID == key then
            if Player.AddItemLua(player, key, value.Number, value.Series, value.bind) == true then
                Player.RemoveItem(player, item:GetID())
                player:AddNotification("Nhận Kỹ năng <color=red>thành công</color>")
            else
                dialog:AddText("Lỗi không có vật phẩm")
            end
            GUI.CloseDialog(player)
        else
            dialog:AddText("Lỗi không có vật phẩm")
        end
    end
    if selectionID == 5 then
        GUI.CloseDialog(player)
    end
    -- ************************** --
    item:FinishUsing(player)
    -- ************************** --
end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi chọn một trong các vật phẩm, và ấn nút Xác nhận cung cấp bởi vật phẩm thông qua Item Dialog
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
--		itemID: number - ID vật phẩm được chọn
-- ****************************************************** --
function RuongBiKip120:OnItemSelected(scene, item, player, itemID)
    -- ************************** --

    -- ************************** --
end
