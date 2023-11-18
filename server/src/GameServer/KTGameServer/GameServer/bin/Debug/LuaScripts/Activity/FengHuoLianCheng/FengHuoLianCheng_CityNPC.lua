-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '000000' bên dưới thành ID tương ứng
local FengHuoLianCheng_CityNPC = Scripts[400040]

--[[
<Summary>
<Description>Hàm này được gọi khi người chơi ấn vào NPC</Description>
<Param Name="scene" Type="GameObject.Scene">Bản đồ hiện tại</Param>
<Param Name="npc" Type="GameObject.NPC">NPC tương ứng</Param>
<Param Name="player" Type="GameObject.Player">Người chơi tương ứng</Param>
<Returns></Returns>
</Summary>
]]
function FengHuoLianCheng_CityNPC:OnOpen(scene, npc, player)
	
	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
	dialog.AddText("<color=yellow>Phong Hỏa Liên Thành</color> là sự kiện diễn ra hàng tuần vào lúc <color=green>12:00:00</color> đến <color=green>12:45:00</color> các ngày <color=yellow>thứ 7</color> và <color=yellow>chủ nhật</color>.")
	dialog.AddText("Mục tiêu là <color=orange>bảo vệ</color> phe thủ thành, cụ thể là <color=green>3 vị nguyên soái</color> khỏi đợt tấn công của phe địch cho đến hết thời gian sự kiện.")
	dialog.AddText("Trong chiến trường, điểm tích lũy sẽ được tính khi <color=orange>giết địch</color> hoặc <color=orange>bảo vệ nguyên soái</color>.")
	dialog.AddText("Trong thời gian thủ thành, nếu cả 3 vị <color=green>nguyên soái</color> đều <color=orange>tử nạn</color>, chiến trường sẽ <color=yellow>kết thúc</color>, người tham gia <color=green>không nhận được</color> phần thưởng nào.")
	dialog.AddText("Bảo vệ <color=green>nguyên soái</color> thành công, hết sự kiện sẽ căn cứ <color=yellow>thứ hạng bản thân</color> nhận quà thưởng sự kiện.")
	
	-- Nếu có phần quà lần trước chưa nhận
	if EventManager.FengHuoLianCheng_HasAward(player) == true then
		dialog.AddSelection(1, "Nhận thưởng sự kiện")
		-- Nếu là thời gian báo danh sự kiện
	elseif EventManager.FengHuoLianCheng_IsRegistrationTime() == true then
		dialog.AddSelection(2, "Báo danh chiến trường")
	end
	
	dialog.AddSelection(9999, "Kết thúc đối thoại")
	dialog.Show(npc, player)
	-- ************************** --
	
end

--[[
<Summary>
<Description>Hàm này được gọi khi có sự kiện người chơi ấn vào một trong số các chức năng cung cấp bởi NPC thông qua NPC Dialog</Description>
<Param Name="scene" Type="GameObject.Scene">Bản đồ hiện tại</Param>
<Param Name="npc" Type="GameObject.Player">NPC tương ứng</Param>
<Param Name="player" Type="GameObject.Player">Người chơi tương ứng</Param>
<Param Name="selectionID" Type="number">ID chức năng</Param>
<Returns></Returns>
</Summary>
]]
function FengHuoLianCheng_CityNPC:OnSelection(scene, npc, player, selectionID)
	
	-- ************************** --
	if selectionID == 9999 then
		GUI.CloseDialog(player)
		return
	end
	-- ************************** --
	if selectionID == 1 then
        -- Nếu không có quà
        if EventManager.FengHuoLianCheng_HasAward(player) == false then
            local dialog = GUI.CreateNPCDialog()
			dialog.AddText("Ngươi không có quà có thể nhận.")
			dialog.AddSelection(9999, "Kết thúc đối thoại")
			dialog.Show(npc, player)
			return
        end

		-- Gọi hàm hệ thống
		local strRet = EventManager.FengHuoLianCheng_GetAward(player)
		-- Toác
		if strRet ~= "OK" then
			local dialog = GUI.CreateNPCDialog()
			dialog.AddText(strRet)
			dialog.AddSelection(9999, "Kết thúc đối thoại")
			dialog.Show(npc, player)
			return
		end
		
		local dialog = GUI.CreateNPCDialog()
		dialog.AddText("Nhận thưởng thành công!")
		dialog.AddSelection(9999, "Kết thúc đối thoại")
		dialog.Show(npc, player)
		return
	end
	-- ************************** --
	if selectionID == 2 then
		-- Nếu không phải thời gian báo danh sự kiện
		if EventManager.FengHuoLianCheng_IsRegistrationTime() == false then
			local dialog = GUI.CreateNPCDialog()
			dialog.AddText("Hiện không phải thời gian báo danh sự kiện, hãy quay lại sau!")
			dialog.AddSelection(9999, "Kết thúc đối thoại")
			dialog.Show(npc, player)
			return
		end

        -- Gọi hàm hệ thống
		local strRet = EventManager.FengHuoLianCheng_Register(player)
		-- Toác
		if strRet ~= "OK" then
			local dialog = GUI.CreateNPCDialog()
			dialog.AddText(strRet)
			dialog.AddSelection(9999, "Kết thúc đối thoại")
			dialog.Show(npc, player)
			return
		end

        -- Đưa đến khu vực chuẩn bị sự kiện
        EventManager.FengHuoLianCheng_BringToBattleOutpost(player)

        -- Đóng khung
        GUI.CloseDialog(player)
		return
	end
	-- ************************** --
	
end

--[[
<Summary>
<Description>Hàm này được gọi khi có sự kiện người chơi chọn một trong các vật phẩm, và ấn nút Xác nhận cung cấp bởi NPC thông qua NPC Dialog</Description>
<Param Name="scene" Type="GameObject.Scene">Bản đồ hiện tại</Param>
<Param Name="npc" Type="GameObject.Player">NPC tương ứng</Param>
<Param Name="player" Type="GameObject.Player">Người chơi tương ứng</Param>
<Param Name="selectedItemInfo" Type="SelectItem">Vật phẩm được chọn</Param>
<Param Name="otherParams" Type="any">Danh sách các tham biến khác</Param>
<Returns></Returns>
</Summary>
]]
function FengHuoLianCheng_CityNPC:OnItemSelected(scene, npc, player, selectedItemInfo, otherParams)
	
	-- ************************** --
	
	-- ************************** --
	
end