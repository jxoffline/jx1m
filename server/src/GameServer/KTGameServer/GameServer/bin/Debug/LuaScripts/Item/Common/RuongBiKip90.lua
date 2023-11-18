-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'IDSkillName ="Kỹ năng 
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '200005' bên dưới thành ID tương ứng
local RuongBiKip90 = Scripts[200108]
--SKILL 120X
local IDSkill = {
    [8489] = {
        RequireFaction = 1,
        RequireLevel = 80,  
        SkillID = 10019, 
        Name ="Kỹ năng Đạt Ma Độ Giang",
        Number =1,
        Series =-1,
        Bind =1,
    },
    [8490] = {
        RequireFaction = 1,
        RequireLevel = 80,
        SkillID = 10027, 
        Name ="Kỹ năng Hoành Tảo Thiên Quâ",
        Number =1,
        Series =-1,
        Bind =1,
    },
    [8491] = {
        RequireFaction = 1,
        RequireLevel = 80,
        SkillID = 10031, 
        Name ="Kỹ năng Vô Tướng Trảm Bí Kíp",
        Number =1,
        Series =-1,
        Bind =1,
    },
    [8492] = {
        RequireFaction = 2,
        RequireLevel = 80,
        SkillID = 10322, 
        Name ="Kỹ năng Phá Thiên Trảm Bí Kíp",
        Number =1,
        Series =-1,
        Bind =1,
    },
    [8493] = {
        RequireFaction = 2,
        RequireLevel = 80,
        SkillID = 10323, 
        Name ="Kỹ năng Truy Tinh Trục Nguyệt",
        Number =1,
        Series =-1,
        Bind =1,
    },
    [8494] = {
        RequireFaction = 2,
        RequireLevel = 80,
        SkillID = 10325, 
        Name ="Kỹ năng Truy Phong Quyết",
        Number =1,
        Series =-1,
        Bind =1,
    },
    [8495] = {
        RequireFaction = 4,
        RequireLevel = 80,
        SkillID = 10107, 
        Name ="Kỹ năng Âm Phong Thực Cốt",
        Number =1,
        Series =-1,
        Bind =1,
    },
    [8496] = {
        RequireFaction = 4,
        RequireLevel = 80,
        SkillID = 10113, 
        Name ="Kỹ năng Huyền Âm Trảm",
        Number =1,
        Series =-1,
        Bind =1,
    },
    [8497] = {
        RequireFaction = 3,
        RequireLevel = 80,
        SkillID = 10137, 
        Name ="Kỹ năng Bạo Vũ Lê Hoa ",
        Number =1,
        Series =-1,
        Bind =1,
    },
    [8498] = {
        RequireFaction = 3,
        RequireLevel = 80,
        SkillID = 10143, 
        Name ="Kỹ năng Cửu Cung Phi Tinh",
        Number =1,
        Series =-1,
        Bind =1,
    },
    [8499] = {
        RequireFaction = 3,
        RequireLevel = 80,
        SkillID = 10130, 
        Name ="Kỹ năng Nhiếp Hồn Nguyệt Ảnh",
        Number =1,
        Series =-1,
        Bind =1,
    },
    [8500] = {
        RequireFaction = 5,
        RequireLevel = 80,
        SkillID = 10161, 
        Name ="Kỹ năng Tam Nga Tề Tuyết",
        Number =1,
        Series =-1,
        Bind =1,
    },
    [8501] = {
        RequireFaction = 5,
        RequireLevel = 80,
        SkillID = 10170, 
        Name ="Kỹ năng Phong Sương Toái Ảnh ",
        Number =1,
        Series =-1,
        Bind =1,
    },
    [8502] = {
        RequireFaction = 6,
        RequireLevel = 80,
        SkillID = 10192, 
        Name ="Kỹ năng Băng Tung Vô Ảnh",
        Number =1,
        Series =-1,
        Bind =1,
    },
    [8503] = {
        RequireFaction = 6,
        RequireLevel = 80,
        SkillID = 10200, 
        Name ="Kỹ năng Băng Tâm Tiên ",
        Number =1,
        Series =-1,
        Bind =1,
    },
    [8504] = {
        RequireFaction = 7,
        RequireLevel = 80,
        SkillID = 10207, 
        Name ="Kỹ năng Phi Long Tại Thiên ",
        Number =1,
        Series =-1,
        Bind =1,
    },
    [8505] = {
        RequireFaction = 7,
        RequireLevel = 80,
        SkillID = 10216, 
        Name ="Kỹ năng Thiên Hạ Vô Cẩu",
        Number =1,
        Series =-1,
        Bind =1,
    },
    [8506] = {
        RequireFaction = 8,
        RequireLevel = 80,
        SkillID = 10229, 
        Name ="Kỹ năng Vân Long Kích ",
        Number =1,
        Series =-1,
        Bind =1,
    },
    [8507] = {
        RequireFaction = 8,
        RequireLevel = 80,
        SkillID = 10236, 
        Name ="Kỹ năng Thiên Ngoại Lưu Tinh ",
        Number =1,
        Series =-1,
        Bind =1,
    },
    [8508] = {
        RequireFaction = 9,
        RequireLevel = 80,
        SkillID = 10250, 
        Name ="Kỹ năng Thiên Địa Vô Cực",
        Number =1,
        Series =-1,
        Bind =1,
    },
    [8509] = {
        RequireFaction = 9,
        RequireLevel = 80,
        SkillID = 10258, 
        Name ="Kỹ năng Nhân Kiếm Hợp Nhất",
        Number =1,
        Series =-1,
        Bind =1,
    },
    [8510] = {
        RequireFaction = 10,
        RequireLevel = 80,
        SkillID = 10273, 
        Name ="Kỹ năng Ngạo Tuyết Tiêu Phong",
        Number =1,
        Series =-1,
        Bind =1,
    },
    [8511] = {
        RequireFaction = 10,
        RequireLevel = 80,
        SkillID = 10280, 
        Name ="Kỹ năng ĐLôi Động Cửu Thiên",
        Number =1,
        Series =-1,
        Bind =1,
    },
    [8512] = {
        RequireFaction = 11,
        RequireLevel = 80,
        SkillID = 11368, 
        Name ="Kỹ năng Đoạt Mệnh Liên Hoàn Tam Tiên Kiế",
        Number =1,
        Series =-1,
        Bind =1,
    },
    [8513] = {
        RequireFaction = 11,
        RequireLevel = 80,
        SkillID = 11370, 
        Name ="Kỹ năng Triệt Thạch Phá Ngọc",
        Number =1,
        Series =-1,
        Bind =1,
    },
    [8514] = {
        RequireFaction = 5,
        RequireLevel = 80,
        SkillID = 10185, 
        Name ="Kỹ năng 10185 Phổ Độ Chúng Sinh",
        Number =1,
        Series =-1,
        Bind =1,
    },
    [8515] = {
        RequireFaction = 3,
        RequireLevel = 80,
        SkillID = 10155, 
        Name ="Kỹ năng Loạn Hoàn Kích",
        Number =1,
        Series =-1,
        Bind =1,
    },
    [8516] = {
        RequireFaction = 4,
        RequireLevel = 80,
        SkillID = 10122, 
        Name ="Kỹ năng Đoạn Cân Hủ Cốt",
        Number =1,
        Series =-1,
        Bind =1,
    },
    [8517] = {
        RequireFaction = 8,
        RequireLevel = 80,
        SkillID = 10244, 
        Name ="Kỹ năng Nhiếp Hồn Loạn Tâm",
        Number =1,
        Series =-1,
        Bind =1,
    },
    [8518] = {
        RequireFaction = 10,
        RequireLevel = 80,
        SkillID = 10295, 
        Name ="Kỹ năng  Túy Tiên Tá Cốt",
        Number =1,
        Series =-1,
        Bind =1,
    },

}

-- ****************************************************** --
--	Hàm này được gọi khi người chơi ấn sử dụng vật phẩm, kiểm tra điều kiện có được dùng hay không
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
--	Return: True nếu thỏa mãn điều kiện có thể sử dụng, False nếu không thỏa mãn
-- ****************************************************** --
function RuongBiKip90:OnPreCheckCondition(scene, item, player, otherParams)
    
    -- ************************** --
    --player:AddNotification("Enter -> RuongBiKip90:OnPreCheckCondition")--
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
function RuongBiKip90:OnUse(scene, item, player, otherParams)
    
    -- ************************** --
    -- ************************** --
    local dialog = GUI.CreateItemDialog()
    dialog:AddText("Dùng được kỹ năng cực phẩm. Hãy chọn kỹ năng mình cần.<br><color=#53f9e8>(Nhận Kỹ năng sẽ tự động khóa)</color>")
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
function RuongBiKip90:OnSelection(scene, item, player, selectionID, otherParams)

    -- ************************** --
    --player:AddNotification("Enter -> RuongBiKip90:OnSelection, selectionID = " .. selectionID)--
    
    local dialog = GUI.CreateItemDialog()
    for key, value in pairs(IDSkill) do
        if selectionID == key then
            if Player.AddItemLua(player, key, value.Number, value.Series, value.bind) ==true then
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
function RuongBiKip90:OnItemSelected(scene, item, player, itemID)

    -- ************************** --

    -- ************************** --

end
