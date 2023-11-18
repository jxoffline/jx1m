-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '200005' bên dưới thành ID tương ứng
local SkillScroll = Scripts[200300]

-- ************************** --
Global_FactionID = {
    -- Không có
    None = 0,
    -- Thiếu Lâm
    ShaoLin = 1,
    -- Thiên Vương
    TianWang = 2,
    -- Đường Môn
    TangMen = 3,
    -- Ngũ Độc
    WuDu = 4,
    -- Nga My
    EMei = 5,
    -- Thúy Yên
    CuiYan = 6,
    -- Cái Bang
    GaiBang = 7,
    -- Thiên Nhẫn
    TianRen = 8,
    -- Võ Đang
    WuDang = 9,
    -- Côn Lôn
    KunLun = 10,
    -- Hoa Son
    HoaSon = 11,
}
-- Danh sách Môn phái
Global_FactionName = {
    [Global_FactionID.None] = "Không có",
    [Global_FactionID.ShaoLin] = "Thiếu Lâm",
    [Global_FactionID.TianWang] = "Thiên Vương",
    [Global_FactionID.TangMen] = "Đường Môn",
    [Global_FactionID.WuDu] = "Ngũ Độc",
    [Global_FactionID.EMei] = "Nga My",
    [Global_FactionID.CuiYan] = "Thúy Yên",
    [Global_FactionID.GaiBang] = "Cái Bang",
    [Global_FactionID.TianRen] = "Thiên Nhẫn",
    [Global_FactionID.WuDang] = "Võ Đang",
    [Global_FactionID.KunLun] = "Côn Lôn",
    [Global_FactionID.HoaSon] = "Hoa Sơn",
}
local SkillList = {
    -- Danh sách ID SKILL 9X
    [8489] = {
        RequireFaction = 1,
        RequireLevel = 80,
        SkillID = 10019, --Đạt Ma Độ Giang
    },
    [8490] = {
        RequireFaction = 1,
        RequireLevel = 80,
        SkillID = 10027, --Hoành Tảo Thiên Quâ
    },
    [8491] = {
        RequireFaction = 1,
        RequireLevel = 80,
        SkillID = 10031, --"Vô Tướng Trảm Bí Kíp
    },
    [8492] = {
        RequireFaction = 2,
        RequireLevel = 80,
        SkillID = 10322, --Phá Thiên Trảm Bí Kíp
    },
    [8493] = {
        RequireFaction = 2,
        RequireLevel = 80,
        SkillID = 10323, --"Truy Tinh Trục Nguyệt
    },
    [8494] = {
        RequireFaction = 2,
        RequireLevel = 80,
        SkillID = 10325, --Truy Phong Quyết
    },
    [8495] = {
        RequireFaction = 4,
        RequireLevel = 80,
        SkillID = 10107, --Âm Phong Thực Cốt
    },
    [8496] = {
        RequireFaction = 4,
        RequireLevel = 80,
        SkillID = 10113, --"Huyền Âm Trảm
    },
    [8497] = {
        RequireFaction = 3,
        RequireLevel = 80,
        SkillID = 10137, --Bạo Vũ Lê Hoa
    },
    [8498] = {
        RequireFaction = 3,
        RequireLevel = 80,
        SkillID = 10143, --Cửu Cung Phi Tinh
    },
    [8499] = {
        RequireFaction = 3,
        RequireLevel = 80,
        SkillID = 10130, --Nhiếp Hồn Nguyệt Ảnh
    },
    [8500] = {
        RequireFaction = 5,
        RequireLevel = 80,
        SkillID = 10161, --Tam Nga Tề Tuyết
    },
    [8501] = {
        RequireFaction = 5,
        RequireLevel = 80,
        SkillID = 10170, --Phong Sương Toái Ảnh
    },
    [8502] = {
        RequireFaction = 6,
        RequireLevel = 80,
        SkillID = 10192, --Băng Tung Vô Ảnh
    },
    [8503] = {
        RequireFaction = 6,
        RequireLevel = 80,
        SkillID = 10200, --"Băng Tâm Tiên
    },
    [8504] = {
        RequireFaction = 7,
        RequireLevel = 80,
        SkillID = 10207, --Phi Long Tại Thiên
    },
    [8505] = {
        RequireFaction = 7,
        RequireLevel = 80,
        SkillID = 10216, --Thiên Hạ Vô Cẩu
    },
    [8506] = {
        RequireFaction = 8,
        RequireLevel = 80,
        SkillID = 10229, --Vân Long Kích
    },
    [8507] = {
        RequireFaction = 8,
        RequireLevel = 80,
        SkillID = 10236, --Thiên Ngoại Lưu Tinh
    },
    [8508] = {
        RequireFaction = 9,
        RequireLevel = 80,
        SkillID = 10250, --Thiên Địa Vô Cực
    },
    [8509] = {
        RequireFaction = 9,
        RequireLevel = 80,
        SkillID = 10258, --Nhân Kiếm Hợp Nhất
    },
    [8510] = {
        RequireFaction = 10,
        RequireLevel = 80,
        SkillID = 10273, --Ngạo Tuyết Tiêu Phong
    },
    [8511] = {
        RequireFaction = 10,
        RequireLevel = 80,
        SkillID = 10280, --ĐLôi Động Cửu Thiên
    },
    [8512] = {
        RequireFaction = 11,
        RequireLevel = 80,
        SkillID = 11368, --Đoạt Mệnh Liên Hoàn Tam Tiên Kiế
    },
    [8513] = {
        RequireFaction = 11,
        RequireLevel = 80,
        SkillID = 11370, --Triệt Thạch Phá Ngọc
    },
    [8514] = {
        RequireFaction = 5,
        RequireLevel = 80,
        SkillID = 10185, --10185 Phổ Độ Chúng Sinh
    },
    [8515] = {
        RequireFaction = 3,
        RequireLevel = 80,
        SkillID = 10155, --Loạn Hoàn Kích
    },
    [8516] = {
        RequireFaction = 4,
        RequireLevel = 80,
        SkillID = 10122, --Đoạn Cân Hủ Cốt
    },
    [8517] = {
        RequireFaction = 8,
        RequireLevel = 80,
        SkillID = 10244, --Nhiếp Hồn Loạn Tâm
    },
    [8518] = {
        RequireFaction = 10,
        RequireLevel = 80,
        SkillID = 10295, -- Túy Tiên Tá Cốt
    },
    [8566] = {
        RequireFaction = 12,
        RequireLevel = 80,
        SkillID = 11394, -- Long thôn thức
    },
    [8567] = {
        RequireFaction = 12,
        RequireLevel = 80,
        SkillID = 11410, -- Thánh hỏa liêu nguyên
    },
    [8572] = {
        RequireFaction = 16,
        RequireLevel = 80,
        SkillID = 1511, --Bí Kíp Nhất Kỵ Đương Thiên 90
    },
    [8573] = {
        RequireFaction = 16,
        RequireLevel = 80,
        SkillID = 1565, --  Phá Phủ Trầm Chu
    },
    --Tiêu dao
    [34618] = {
        RequireFaction = 15,
        RequireLevel = 80,
        SkillID = 1411, --Bạch Nhật Sâm Thần 90"
    },
    [34619] = {
        RequireFaction = 15,
        RequireLevel = 80,
        SkillID = 1464, --  Tê Chiếu Phồn Thương 90 "
    },

    
    -- **************************--
    --SKILL 120X
    [8519] = {
        RequireFaction = 1,
        RequireLevel = 120,
        SkillID = 10022, --Đại Thừa Như Lai Chú
    },
    [8520] = {
        RequireFaction = 2,
        RequireLevel = 120,
        SkillID = 10052, -- Đảo Hư Thiên
    },
    [8521] = {
        RequireFaction = 4,
        RequireLevel = 120,
        SkillID = 10098, -- Hấp Tinh Yểm
    },
    [8522] = {
        RequireFaction = 3,
        RequireLevel = 120,
        SkillID = 10157, -- Mê Ảnh Tung
    },
    [8523] = {
        RequireFaction = 5,
        RequireLevel = 120,
        SkillID = 10176, -- Bế Nguyệt Phất Trần
    },
    [8524] = {
        RequireFaction = 6,
        RequireLevel = 120,
        SkillID = 10085, -- Ngự Tuyết Ẩn"
    },
    [8525] = {
        RequireFaction = 7,
        RequireLevel = 120,
        SkillID = 10083, -- "Hỗn Thiên Khí Công
    },
    [8526] = {
        RequireFaction = 8,
        RequireLevel = 120,
        SkillID = 10080, -- Ma Âm Phệ Phách
    },
    [8527] = {
        RequireFaction = 9,
        RequireLevel = 120,
        SkillID = 10267, --Xuất Ứ Bất Nhiễm
    },
    [8528] = {
        RequireFaction = 10,
        RequireLevel = 120,
        SkillID = 10296, -- lưỡng Nghi Chân Khí
    },
    [8529] = {
        RequireFaction = 11,
        RequireLevel = 120,
        SkillID = 11377, -- Hạo Nhiên Chi Khí
    },
    [8530] = {
        RequireFaction = 11,
        RequireLevel = 120,
        SkillID = 11371, -- Tử Hà Kiếm Khí
    },
    [8568] = {
        RequireFaction = 12,
        RequireLevel = 120,
        SkillID = 11397 -- Không tuyệt tâm pháp
    },
    [8569] = {
        RequireFaction = 12,
        RequireLevel = 120,
        SkillID = 11411 --thánh hỏa thần công 120
    },

    ----------------------------------------------------------------
    [8574] = {
        RequireFaction = 16,
        RequireLevel = 120,
        SkillID = 1513, --Càn Nguyên Trấn Hải Quyết
    },
    [8575] = {
        RequireFaction = 16,
        RequireLevel = 120,
        SkillID = 1516, --Binh Phong Trực Chỉ 120 "
    },
    [8576] = {
        RequireFaction = 16,
        RequireLevel = 120,
        SkillID = 1569 -- Lôi Tự Kim Bình 120"
    },
    [8577] = {
        RequireFaction = 16,
        RequireLevel = 120,
        SkillID = 1566 --Ngạo Khí Hàn Sương Quyết
    },
    --Tiêu dao
    [34607] = {
        RequireFaction = 15,
        RequireLevel = 120,
        SkillID = 1413 --  Phục Nhật Xuất Vân Bí Kíp 120"
    },
    [34608] = {
        RequireFaction = 15,
        RequireLevel = 120,
        SkillID = 1417 -- Hỗn Nhật Khí Quyết Bí Kíp 120
    },
    [34609] = {
        RequireFaction = 15,
        RequireLevel = 120,
        SkillID = 1467 --  Hỏa Hải Vô Nhai Bí Kíp 120"
    },
    [34610] = {
        RequireFaction = 15,
        RequireLevel = 120,
        SkillID = 1465 --Phần Phách Tru Tâm Bí Kíp 120
    },
    --===============================
    --150x
    [8437] = {
        RequireFaction = 1,
        RequireLevel = 140,
        SkillID = 10028, -- Vi Đà Hiến Xử
    },
    [8438] = {
        RequireFaction = 1,
        RequireLevel = 140,
        SkillID = 10032, -- Tam Giới Quy Thiền
    },
    [8439] = {
        RequireFaction = 1,
        RequireLevel = 140,
        SkillID = 10021, -- Đại Lực Kim Cang Chưởng
    },
    [8440] = {
        RequireFaction = 2,
        RequireLevel = 140,
        SkillID = 11057, -- Hào Hùng Trảm
    },
    [8441] = {
        RequireFaction = 2,
        RequireLevel = 140,
        SkillID = 11060, -- Tung Hoành Bát Hoang
    },
    [8442] = {
        RequireFaction = 2,
        RequireLevel = 140,
        SkillID = 11061, -- Bá Vương Tạm Kim
    },
    [8443] = {
        RequireFaction = 4,
        RequireLevel = 140,
        SkillID = 10109, -- "Hình Tiêu Cốt Lập
    },
    [8444] = {
        RequireFaction = 4,
        RequireLevel = 140,
        SkillID = 10115, -- U Hồn Phệ Ảnh
    },
    [8445] = {
        RequireFaction = 3,
        RequireLevel = 140,
        SkillID = 10139, -- Thiết Liên Tứ Sát
    },
    [8446] = {
        RequireFaction = 3,
        RequireLevel = 140,
        SkillID = 10144, -- Càn Khôn Nhất Trịch
    },
    [8447] = {
        RequireFaction = 3,
        RequireLevel = 140,
        SkillID = 10129, -- Vô Ảnh Xuyên
    },
    [8448] = {
        RequireFaction = 5,
        RequireLevel = 140,
        SkillID = 10163, -- Kiếm Hoa Vãn Tinh
    },
    [8449] = {
        RequireFaction = 5,
        RequireLevel = 140,
        SkillID = 10172, -- Băng Vũ Lạc Tinh
    },
    [8450] = {
        RequireFaction = 5,
        RequireLevel = 140,
        SkillID = 10092, -- Ngọc Tuyền Tâm Kinh
    },
    [8451] = {
        RequireFaction = 6,
        RequireLevel = 140,
        SkillID = 10194, -- "Băng Tước Hoạt Kỳ
    },
    [8452] = {
        RequireFaction = 6,
        RequireLevel = 140,
        SkillID = 10202, -- Thủy Anh Man Tú
    },
    [8453] = {
        RequireFaction = 7,
        RequireLevel = 140,
        SkillID = 10209, -- Thời Thặng Lục Long
    },
    [8454] = {
        RequireFaction = 7,
        RequireLevel = 140,
        SkillID = 10217, -- Bổng Huýnh Lược Địa
    },
    [8455] = {
        RequireFaction = 8,
        RequireLevel = 140,
        SkillID = 10230, -- Giang Hải Nộ Lan
    },
    [8456] = {
        RequireFaction = 8,
        RequireLevel = 140,
        SkillID = 10238, -- Tật Hỏa Liệu Nguyên
    },
    [8457] = {
        RequireFaction = 9,
        RequireLevel = 140,
        SkillID = 10251, -- Tạo Hóa Thái Thanh
    },
    [8458] = {
        RequireFaction = 9,
        RequireLevel = 140,
        SkillID = 10261, -- "Kiếm Thùy Tinh Hà
    },
    [8459] = {
        RequireFaction = 10,
        RequireLevel = 140,
        SkillID = 10274, -- Cửu Thiên Cương Phong
    },
    [8460] = {
        RequireFaction = 10,
        RequireLevel = 140,
        SkillID = 10281, -- Thiên Lôi Chấn Nhạc
    },
    [8461] = {
        RequireFaction = 11,
        RequireLevel = 140,
        SkillID = 11373, -- Cửu Kiếm Hợp Nhất
    },
    [8462] = {
        RequireFaction = 11,
        RequireLevel = 140,
        SkillID = 11375, -- Thần Quang Toàn Nhiễu
    },
    [8570] = {
        RequireFaction = 12,
        RequireLevel = 140,
        SkillID = 11398, --   cửu hi hỗn dương
    },
    [8571] = {
        RequireFaction = 12,
        RequireLevel = 140,
        SkillID = 11412, -- thánh hỏa lệnh pháp
    },

    ----------------------------------------------------------------
    [8578] = {
        RequireFaction = 16,
        RequireLevel = 140,
        SkillID = 1514, -- Bí Kíp Dục Huyết Thao Phong
    },
    [8579] = {
        RequireFaction = 16,
        RequireLevel = 140,
        SkillID = 1517, -- Bí Kíp Hiệp Sơn Siêu Hải  "
    },
    [8580] = {
        RequireFaction = 16,
        RequireLevel = 140,
        SkillID = 1567, -- Bí Kíp Thừa Thắng Trục Bắc
    },
    [8581] = {
        RequireFaction = 16,
        RequireLevel = 140,
        SkillID = 1570, -- Bí Kíp Nghịch Huyết Tiệt Mạch
    },
    --Tiêu dao
    [34613] = {
        RequireFaction = 15,
        RequireLevel = 140,
        SkillID = 1414, -- Bí Kíp Sinh Tử Phù 150 "
    },
    [34614] = {
        RequireFaction = 15,
        RequireLevel = 140,
        SkillID = 1419, -- Bí Kíp Thái Hư Thần Công 150  "
    },
    [34615] = {
        RequireFaction = 15,
        RequireLevel = 140,
        SkillID = 1466, -- Bí Kíp Ngang Nhật Đồ 150
    },
    [34616] = {
        RequireFaction = 15,
        RequireLevel = 140,
        SkillID = 1468, -- Bí Kíp Bính Nhược Quan Hỏa
    },
    --[[
        ItemID="34607"
                Name=" Phục Nhật Xuất Vân Bí Kíp 120
        ItemID="34608"
                Name="Hỗn Nhật Khí Quyết Bí Kíp 120
        ItemID="34609"
                Name=" Hỏa Hải Vô Nhai Bí Kíp 120
        ItemID="34610"
                Name=" Phần Phách Tru Tâm Bí Kíp 120
        ItemID="34613"
                Name=" Bí Kíp Sinh Tử Phù 150 "
        ItemID="34614"
                Name=" Bí Kíp Thái Hư Thần Công 150
        ItemID="34615"
                Name=" Bí Kíp Ngang Nhật Đồ 150 "
        ItemID="34616"
                Name=" Bí Kíp Bính Nhược Quan Hỏa 150
        ItemID="34618"
                Name="Bạch Nhật Sâm Thần 90"
        ItemID="34619"
                Name="Tê Chiếu Phồn Thương 90 "

        ID="1411" Name="Bạch Nhật Sâm Thần" 90
        ID="1413" Name="Phục Nhật Xuất Vân" 120
        ID="1417" Name="Hỗn Nhật Khí Quyết" 120
        ID="1414" Name="Sinh Tử Phù" 150
        ID="1419" Name="Thái Hư Thần Công" 150
        ID="1464" Name="Tê Chiếu Phồn Thương" 90
        ID="1465" Name="Phần Phách Tru Tâm" 120
        ID="1467" Name="Hỏa Hải Vô Nhai" 120
        ID="1466" Name="Ngang Nhật Đồ" 150
        ID="1468" Name="Bính Nhược Quan Hỏa" 150
    ]]

}
-- ************************** --


--[[
<Summary>
<Description>Hàm này được gọi khi người chơi ấn sử dụng vật phẩm, kiểm tra điều kiện có được dùng hay không</Description>
<Param Name="scene" Type="GameObject.Scene">Bản đồ hiện tại</Param>
<Param Name="item" Type="GameObject.Item">Vật phẩm tương ứng</Param>
<Param Name="player" Type="GameObject.Player">Người chơi tương ứng</Param>
<Param Name="otherParams" Type="any">Các tham biến khác</Param>
<Returns></Returns>
</Summary>
]]
function SkillScroll:OnPreCheckCondition(scene, item, player, otherParams)
    -- ************************** --
    -- Nếu vật phẩm không tồn tại
    if SkillList[item:GetItemID()] == nil then
        player:AddNotification("Vật phẩm không hợp lệ!")
        return false
    end

    -- Thông tin kỹ năng
    local skillInfo = SkillList[item:GetItemID()]

    -- Nếu cấp độ không đủ
    if player:GetLevel() < skillInfo.RequireLevel then
        player:AddNotification(string.format("Bạn cần đạt tối thiểu cấp độ %d mới có thể học kỹ năng này!",
            skillInfo.RequireLevel))
        return false
    end

    -- Nếu Môn phái không phù hợp
    if skillInfo.RequireFaction ~= -1 and player:GetFactionID() ~= skillInfo.RequireFaction then
        player:AddNotification("Môn phái không phù hợp để học bí kíp này !")
        for key, value in pairs(Global_FactionName) do
            if skillInfo.RequireFaction == key then
                player:AddNotification("Bí kíp này chỉ có Môn phái " .. value .. " sử dụng được!")
            end
            -- if player:GetFactionID() == key then
            --  player:AddNotification("Ngươi đang ở phái "..value.." hãy mua các bí kí của "..value.."!")
            --end
        end
        return false
    end

    -- Nếu đã học kỹ năng này rồi
    if player:GetSkillLevel(skillInfo.SkillID) > 0 then
        player:AddNotification("Bạn đã học kỹ năng này rồi!")
        return false
    end

    -- OK
    return true
    -- ************************** --
end

--[[
<Summary>
<Description>Hàm này được gọi để thực thi Logic khi người sử dụng vật phẩm, sau khi đã thỏa mãn hàm kiểm tra điều kiện</Description>
<Param Name="scene" Type="GameObject.Scene">Bản đồ hiện tại</Param>
<Param Name="item" Type="GameObject.Item">Vật phẩm tương ứng</Param>
<Param Name="player" Type="GameObject.Player">Người chơi tương ứng</Param>
<Param Name="otherParams" Type="any">Các tham biến khác</Param>
<Returns></Returns>
</Summary>
]]
function SkillScroll:OnUse(scene, item, player, otherParams)
    -- ************************** --
    -- Nếu vật phẩm không tồn tại
    if SkillList[item:GetItemID()] == nil then
        player:AddNotification("Vật phẩm không hợp lệ!")
        return
    end

    -- Thông tin kỹ năng
    local skillInfo = SkillList[item:GetItemID()]

    -- Nếu cấp độ không đủ
    if player:GetLevel() < skillInfo.RequireLevel then
        player:AddNotification(string.format("Bạn cần đạt tối thiểu cấp độ %d mới có thể học kỹ năng này!",
            skillInfo.RequireLevel))
        return
    end

    -- Nếu Môn phái không phù hợp
    if skillInfo.RequireFaction ~= -1 and player:GetFactionID() ~= skillInfo.RequireFaction then
        player:AddNotification("Môn phái không phù hợp!")
        return
    end

    -- Nếu đã học kỹ năng này rồi
    if player:GetSkillLevel(skillInfo.SkillID) > 0 then
        player:AddNotification("Bạn đã học kỹ năng này rồi!")
        return
    end

    -- Nếu chưa có kỹ năng này
    if player:HasSkill(skillInfo.SkillID) == false then
        -- Thêm kỹ năng vào
        player:AddSkill(skillInfo.SkillID)
    end
    -- Thực hiện xóa vật phẩm
    item:FinishUsing(player)
    -- Thêm kỹ năng tương ứng với cấp độ 1
    player:AddSkillLevel(skillInfo.SkillID, 1)

    -- Thông báo học thành công
    player:AddNotification("Học kỹ năng thành công!")
    -- ************************** --
end

--[[
<Summary>
<Description>Hàm này được gọi khi có sự kiện người chơi ấn vào một trong số các chức năng cung cấp bởi vật phẩm thông qua Item Dialog</Description>
<Param Name="scene" Type="GameObject.Scene">Bản đồ hiện tại</Param>
<Param Name="item" Type="GameObject.Item">Vật phẩm tương ứng</Param>
<Param Name="player" Type="GameObject.Player">Người chơi tương ứng</Param>
<Param Name="selectionID" Type="number">ID chức năng</Param>
<Param Name="otherParams" Type="any">Các tham biến khác</Param>
<Returns></Returns>
</Summary>
]]
function SkillScroll:OnSelection(scene, item, player, selectionID, otherParams)
    -- ************************** --
    -- ************************** --
end

--[[
<Summary>
<Description>Hàm này được gọi khi có sự kiện người chơi chọn một trong các vật phẩm, và ấn nút Xác nhận cung cấp bởi vật phẩm thông qua Item Dialog</Description>
<Param Name="scene" Type="GameObject.Scene">Bản đồ hiện tại</Param>
<Param Name="item" Type="GameObject.Item">Vật phẩm tương ứng</Param>
<Param Name="player" Type="GameObject.Player">Người chơi tương ứng</Param>
<Param Name="itemID" Type="numbrt">ID vật phẩm được chọn</Param>
<Returns></Returns>
</Summary>
]]
function SkillScroll:OnItemSelected(scene, item, player, itemID)
    -- ************************** --
    -- ************************** --
end
