-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'IDSkill
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '200005' bên dưới thành ID tương ứng
local RuongBiKip150 = Scripts[200110]
--SKILL 150X
local IDSkill = {
    [8437] = {
        RequireFaction = 1,
        RequireLevel = 140,
        SkillID = 10028,
        Name = "Kỹ năng Vi Đà Hiến Xử",
        Number = 1,
        Series = -1,
        bind = 0,     --không khóa
        bindlock = 1, --khóa
    },
    [8438] = {
        RequireFaction = 1,
        RequireLevel = 140,
        SkillID = 10032,
        Name = "Kỹ năng Tam Giới Quy Thiền",
        Number = 1,
        Series = -1,
        bind = 0,     --không khóa
        bindlock = 1, --khóa
    },
    [8439] = {
        RequireFaction = 1,
        RequireLevel = 140,
        SkillID = 10021,
        Name = "Kỹ năng Đại Lực Kim Cang Chưởng",
        Number = 1,
        Series = -1,
        bind = 0,     --không khóa
        bindlock = 1, --khóa
    },
    [8440] = {
        RequireFaction = 2,
        RequireLevel = 140,
        SkillID = 11057,
        Name = "Kỹ năng Hào Hùng Trảm",
        Number = 1,
        Series = -1,
        bind = 0,     --không khóa
        bindlock = 1, --khóa
    },
    [8441] = {
        RequireFaction = 2,
        RequireLevel = 140,
        SkillID = 11060,
        Name = "Kỹ năng Tung Hoành Bát Hoang",
        Number = 1,
        Series = -1,
        bind = 0,     --không khóa
        bindlock = 1, --khóa
    },
    [8442] = {
        RequireFaction = 2,
        RequireLevel = 140,
        SkillID = 11061,
        Name = "Kỹ năng Bá Vương Tạm Kim",
        Number = 1,
        Series = -1,
        bind = 0,     --không khóa
        bindlock = 1, --khóa
    },
    [8443] = {
        RequireFaction = 4,
        RequireLevel = 140,
        SkillID = 10109,
        Name = "Kỹ năng Hình Tiêu Cốt Lập",
        Number = 1,
        Series = -1,
        bind = 0,     --không khóa
        bindlock = 1, --khóa
    },
    [8444] = {
        RequireFaction = 4,
        RequireLevel = 140,
        SkillID = 10115,
        Name = "Kỹ năng U Hồn Phệ Ảnh",
        Number = 1,
        Series = -1,
        bind = 0,     --không khóa
        bindlock = 1, --khóa
    },
    [8445] = {
        RequireFaction = 3,
        RequireLevel = 140,
        SkillID = 10139,
        Name = "Kỹ năng Thiết Liên Tứ Sát",
        Number = 1,
        Series = -1,
        bind = 0,     --không khóa
        bindlock = 1, --khóa
    },
    [8446] = {
        RequireFaction = 3,
        RequireLevel = 140,
        SkillID = 10144,
        Name = "Kỹ năng Càn Khôn Nhất Trịch",
        Number = 1,
        Series = -1,
        bind = 0,     --không khóa
        bindlock = 1, --khóa
    },
    [8447] = {
        RequireFaction = 3,
        RequireLevel = 140,
        SkillID = 10129,
        Name = "Kỹ năng Vô Ảnh Xuyên",
        Number = 1,
        Series = -1,
        bind = 0,     --không khóa
        bindlock = 1, --khóa
    },
    [8448] = {
        RequireFaction = 5,
        RequireLevel = 140,
        SkillID = 10163,
        Name = "Kỹ năng Kiếm Hoa Vãn Tinh",
        Number = 1,
        Series = -1,
        bind = 0,     --không khóa
        bindlock = 1, --khóa
    },
    [8449] = {
        RequireFaction = 5,
        RequireLevel = 140,
        SkillID = 10172,
        Name = "Kỹ năng Băng Vũ Lạc Tinh",
        Number = 1,
        Series = -1,
        bind = 0,     --không khóa
        bindlock = 1, --khóa
    },
    [8450] = {
        RequireFaction = 5,
        RequireLevel = 140,
        SkillID = 10092,
        Name = "Kỹ năng Ngọc Tuyền Tâm Kinh",
        Number = 1,
        Series = -1,
        bind = 0,     --không khóa
        bindlock = 1, --khóa
    },
    [8451] = {
        RequireFaction = 6,
        RequireLevel = 140,
        SkillID = 10194,
        Name = "Kỹ năng Băng Tước Hoạt Kỳ",
        Number = 1,
        Series = -1,
        bind = 0,     --không khóa
        bindlock = 1, --khóa
    },
    [8452] = {
        RequireFaction = 6,
        RequireLevel = 140,
        SkillID = 10202,
        Name = "Kỹ năng Thủy Anh Man Tú",
        Number = 1,
        Series = -1,
        bind = 0,     --không khóa
        bindlock = 1, --khóa
    },
    [8453] = {
        RequireFaction = 7,
        RequireLevel = 140,
        SkillID = 10209,
        Name = "Kỹ năng Thời Thặng Lục Long ",
        Number = 1,
        Series = -1,
        bind = 0,     --không khóa
        bindlock = 1, --khóa
    },
    [8454] = {
        RequireFaction = 7,
        RequireLevel = 140,
        SkillID = 10217,
        Name = "Kỹ năng Bổng Huýnh Lược Địa",
        Number = 1,
        Series = -1,
        bind = 0,     --không khóa
        bindlock = 1, --khóa
    },
    [8455] = {
        RequireFaction = 8,
        RequireLevel = 140,
        SkillID = 10230,
        Name = "Kỹ năng Giang Hải Nộ Lan",
        Number = 1,
        Series = -1,
        bind = 0,     --không khóa
        bindlock = 1, --khóa
    },
    [8456] = {
        RequireFaction = 8,
        RequireLevel = 140,
        SkillID = 10238,
        Name = "Kỹ năng Tật Hỏa Liệu Nguyên",
        Number = 1,
        Series = -1,
        bind = 0,     --không khóa
        bindlock = 1, --khóa
    },
    [8457] = {
        RequireFaction = 9,
        RequireLevel = 140,
        SkillID = 10251,
        Name = "Kỹ năng Tạo Hóa Thái Thanh",
        Number = 1,
        Series = -1,
        bind = 0,     --không khóa
        bindlock = 1, --khóa
    },
    [8458] = {
        RequireFaction = 9,
        RequireLevel = 140,
        SkillID = 10261,
        Name = "Kỹ năng Kiếm Thùy Tinh Hà",
        Number = 1,
        Series = -1,
        bind = 0,     --không khóa
        bindlock = 1, --khóa
    },
    [8459] = {
        RequireFaction = 10,
        RequireLevel = 140,
        SkillID = 10274,
        Name = "Kỹ năng Cửu Thiên Cương Phong",
        Number = 1,
        Series = -1,
        bind = 0,     --không khóa
        bindlock = 1, --khóa
    },
    [8460] = {
        RequireFaction = 10,
        RequireLevel = 140,
        SkillID = 10281,
        Name = "Kỹ năng Thiên Lôi Chấn Nhạc ",
        Number = 1,
        Series = -1,
        bind = 0,     --không khóa
        bindlock = 1, --khóa
    },
    [8461] = {
        RequireFaction = 11,
        RequireLevel = 140,
        SkillID = 11373,
        Name = "Kỹ năng Cửu Kiếm Hợp Nhất",
        Number = 1,
        Series = -1,
        bind = 0,     --không khóa
        bindlock = 1, --khóa
    },
    [8462] = {
        RequireFaction = 11,
        RequireLevel = 140,
        SkillID = 11375,
        Name = "Kỹ năng Thần Quang Toàn Nhiễu",
        Number = 1,
        Series = -1,
        bind = 0,     --không khóa
        bindlock = 1, --khóa
    },

    [8570] = {
        RequireFaction = 12,
        RequireLevel = 140,
        SkillID = 11398,
        Name = "Kỹ năng  cửu hi hỗn dương",
        Number = 1,
        Series = -1,
        bind = 0,     --không khóa
        bindlock = 1, --khóa
    },
    [8571] = {
        RequireFaction = 12,
        RequireLevel = 140,
        SkillID = 11412,
        Name = "Kỹ năng thánh hỏa lệnh pháp",
        Number = 1,
        Series = -1,
        bind = 0,     --không khóa
        bindlock = 1, --khóa
    },
    [8578] = {
        RequireFaction = 16,
        RequireLevel = 140,
        SkillID = 1514,
        Name = "Bí kip Dục Huyết Thao Phong ",
        Number = 1,
        Series = -1,
        bind = 0,     --không khóa
        bindlock = 1, --khóa
    },
    [8579] = {
        RequireFaction = 16,
        RequireLevel = 140,
        SkillID = 1517,
        Name = "Bí kip Hiệp Sơn Siêu Hải  ",
        Number = 1,
        Series = -1,
        bind = 0,     --không khóa
        bindlock = 1, --khóa
    },
    [8580] = {
        RequireFaction = 16,
        RequireLevel = 140,
        SkillID = 1567,
        Name = "Bí kip Thừa Thắng Trục Bắc  ",
        Number = 1,
        Series = -1,
        bind = 0,     --không khóa
        bindlock = 1, --khóa
    },
    [8581] = {
        RequireFaction = 16,
        RequireLevel = 140,
        SkillID = 1570,
        Name = "Bí kip Nghịch Huyết Tiệt Mạch ",
        Number = 1,
        Series = -1,
        bind = 0,     --không khóa
        bindlock = 1, --khóa
    },
    ----------------Tiêu dao------------------------------------------------
    [34613] = {
        RequireFaction = 15,
        RequireLevel = 140,
        SkillID = 1414,
        Name = " Bí Kíp Sinh Tử Phù ",
        Number = 1,
        Series = -1,
        bind = 0,     --không khóa
        bindlock = 1, --khóa
    },
    [34614] = {
        RequireFaction = 15,
        RequireLevel = 140,
        SkillID = 1419,
        Name = " Bí Kíp Thái Hư Thần Công",
        Number = 1,
        Series = -1,
        bind = 0,     --không khóa
        bindlock = 1, --khóa
    },
    [34615] = {
        RequireFaction = 15,
        RequireLevel = 140,
        SkillID = 1466,
        Name = " Bí Kíp Ngang Nhật Đồ ",
        Number = 1,
        Series = -1,
        bind = 0,     --không khóa
        bindlock = 1, --khóa
    },
    [34616] = {
        RequireFaction = 15,
        RequireLevel = 140,
        SkillID = 1468,
        Name = " Bí Kíp Bính Nhược Quan Hỏa",
        Number = 1,
        Series = -1,
        bind = 0,     --không khóa
        bindlock = 1, --khóa
    },
    -- [8463] = {
    --      RequireFaction = 3,
    --      RequireLevel = 140,
    --       SkillID = 11375,
    --       Name = "Kỹ năng Tích Lịch Loạn Hoàn Kích",
    --    Number = 1,
    --     Series = -1,
    --      bind=0,--không khóa
    --      bindlock=1,--khóa
    --  },
}

-- ****************************************************** --
--	Hàm này được gọi khi người chơi ấn sử dụng vật phẩm, kiểm tra điều kiện có được dùng hay không
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứngStack
--	Return: True nếu thỏa mãn điều kiện có thể sử dụng, False nếu không thỏa mãn
-- ****************************************************** --
function RuongBiKip150:OnPreCheckCondition(scene, item, player, otherParams)
    -- ************************** --
    --player:AddNotification("Enter -> RuongBiKip150:OnPreCheckCondition")--

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
function RuongBiKip150:OnUse(scene, item, player, otherParams)
    -- ************************** --
    -- ************************** --
    local nLevel = item:GetItemLevel()

    if (nLevel < 1 or nLevel > 2) then
        player:AddNotification("Vật phẩm không hợp lệ!")
        return;
    end
    if Player.HasFreeBagSpaces(player, 5) == false then
        player:AddNotification(
            "Túi của ngươi đã đầy, hãy sắp xếp <color=green>5 ô trống</color> trong túi để nhận kỹ năng 150x!")
        return
    end
    local dialog = GUI.CreateItemDialog()
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
function RuongBiKip150:OnSelection(scene, item, player, selectionID, otherParams)
    -- ************************** --
    --player:AddNotification("Enter -> RuongBiKip150:OnSelection, selectionID = " .. selectionID)--
    --lấy level của vật phẩm
    local nLevel = item:GetItemLevel()

    if (nLevel < 1 or nLevel > 2) then
        player:AddNotification("Vật phẩm không hợp lệ!")
        return;
    end
    if Player.HasFreeBagSpaces(player, 5) == false then
        player:AddNotification(
            "Túi của ngươi đã đầy, hãy sắp xếp <color=green>5 ô trống</color> trong túi để nhận kỹ năng 150x!")
        return
    end

    local dialog = GUI.CreateItemDialog()
    for key, value in pairs(IDSkill) do
        if selectionID == key then
            --ckeck nếu là Level là level 1 thì không khóa
            if (nLevel == 1) then
                if Player.AddItemLua(player, key, value.Number, value.Series, value.bind) == true then
                    Player.RemoveItem(player, item:GetID())
                    player:AddNotification("Nhận Kỹ năng <color=red>thành công</color>")
                else
                    dialog:AddText("Lỗi không có vật phẩm")
                end
            else --còn lại là khóa
                if Player.AddItemLua(player, key, value.Number, value.Series, value.bindlock) == true then
                    Player.RemoveItem(player, item:GetID())
                    player:AddNotification("Nhận Kỹ năng <color=red>thành công</color>")
                else
                    dialog:AddText("Lỗi không có vật phẩm")
                end
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
function RuongBiKip150:OnItemSelected(scene, item, player, itemID)
    -- ************************** --

    -- ************************** --
end
