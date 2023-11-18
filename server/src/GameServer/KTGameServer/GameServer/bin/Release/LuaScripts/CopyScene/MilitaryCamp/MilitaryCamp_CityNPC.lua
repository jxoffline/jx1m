-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '000000' bên dưới thành ID tương ứng
local MilitaryCamp_CityNPC = Scripts[500100]

--[[
<Summary>
<Description>Hàm này được gọi khi người chơi ấn vào NPC</Description>
<Param Name="scene" Type="GameObject.Scene">Bản đồ hiện tại</Param>
<Param Name="npc" Type="GameObject.NPC">NPC tương ứng</Param>
<Param Name="player" Type="GameObject.Player">Người chơi tương ứng</Param>
<Returns></Returns>
</Summary>
]]
function MilitaryCamp_CityNPC:OnOpen(scene, npc, player)
	
	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
	dialog.AddText("Nghĩa quân đang chiêu mộ anh hùng hào kiệt khắp chốn võ lâm giang hồ. Ngươi trông rất có tố chất, rất xứng đáng gia nhập nghĩa quân. Nếu muốn, mỗi ngày ngươi đều có thể tham gia tập luyện tại thao trường. Muốn tham gia không?")
	dialog.AddSelection(1, "Hậu Sơn Phục Ngưu")
	dialog.AddSelection(2, "Bách Man Sơn")
	dialog.AddSelection(3, "Hải Lăng Vương Mộ")
	dialog.AddSelection(9999, "Kết thúc đối thoại")
	dialog.Show(npc, player)
	-- ************************** --
	
end

--[[
<Summary>
<Description>Hàm này được gọi khi có sự kiện người chơi ấn vào một trong số các chức năng cung cấp bởi NPC thông qua NPC Dialog</Description>
<Param Name="scene" Type="GameObject.Scene">Bản đồ hiện tại</Param>
<Param Name="npc" Type="GameObject.NPC">NPC tương ứng</Param>
<Param Name="player" Type="GameObject.Player">Người chơi tương ứng</Param>
<Param Name="selectionID" Type="number">ID chức năng</Param>
<Returns></Returns>
</Summary>
]]
function MilitaryCamp_CityNPC:OnSelection(scene, npc, player, selectionID)
	
	-- ************************** --
	if selectionID == 9999 then
		GUI.CloseDialog(player)
		return
	end
	-- ************************** --
	if selectionID >= 1 and selectionID <= 3 then
		-- Loại phụ bản
		local nIndex = selectionID - 1
		-- Gọi hàm hệ thống
		local strRet = EventManager.MilitaryCamp_CheckCondition(player, nIndex)
		-- Toác
		if strRet ~= "OK" then
			local dialog = GUI.CreateNPCDialog()
			dialog.AddText(strRet)
			dialog.AddSelection(9999, "Kết thúc đối thoại")
			dialog.Show(npc, player)
			return
		end
		
		-- Bắt đầu phụ bản
		EventManager.MilitaryCamp_Begin(player, nIndex)
		return
	end
	-- ************************** --
	
end

--[[
<Summary>
<Description>Hàm này được gọi khi có sự kiện người chơi chọn một trong các vật phẩm, và ấn nút Xác nhận cung cấp bởi NPC thông qua NPC Dialog</Description>
<Param Name="scene" Type="GameObject.Scene">Bản đồ hiện tại</Param>
<Param Name="npc" Type="GameObject.NPC">NPC tương ứng</Param>
<Param Name="player" Type="GameObject.Player">Người chơi tương ứng</Param>
<Param Name="selectedItemInfo" Type="GameObject.Item">Vật phẩm được chọn</Param>
<Returns></Returns>
</Summary>
]]
function MilitaryCamp_CityNPC:OnItemSelected(scene, npc, player, selectedItemInfo)
	
	-- ************************** --
	
	-- ************************** --
	
end