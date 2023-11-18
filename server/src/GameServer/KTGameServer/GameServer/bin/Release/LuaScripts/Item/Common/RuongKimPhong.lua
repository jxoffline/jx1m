-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '200005' bên dưới thành ID tương ứng
local RuongKimPhong = Scripts[200101]

-- ************************** --
local ItemChess = {
    [1566] = {id= 1566, Time =10080 , Series=-1, Bound =1,enhanceLevel=4, Number=1 }
	,
	[1567] = {id= 1567, Time =10080 , Series=-1, Bound =1,enhanceLevel=4, Number=1 }
	,
	[1568] = {id= 1568, Time =10080 , Series=-1, Bound =1,enhanceLevel=4, Number=1 }
	,
	[1569] = {id= 1569, Time =10080 , Series=-1, Bound =1,enhanceLevel=4, Number=1 }
	,
	[1570] = {id= 1570, Time =10080 , Series=-1, Bound =1,enhanceLevel=4, Number=1 }
	,
	[1571] = {id= 1571, Time =10080 , Series=-1, Bound =1,enhanceLevel=4, Number=1 }
	,
	[1572] = {id= 1572, Time =10080 , Series=-1, Bound =1,enhanceLevel=4, Number=1 }
	,
	[1573] = {id= 1573, Time =10080 , Series=-1, Bound =1,enhanceLevel=4, Number=1 }
	,
	[1574] = {id= 1574, Time =10080 , Series=-1, Bound =1,enhanceLevel=4, Number=1 }
	,
}

-- ************************** --

-- ****************************************************** --
--	Hàm này được gọi khi người chơi ấn sử dụng vật phẩm, kiểm tra điều kiện có được dùng hay không
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
--	Return: True nếu thỏa mãn điều kiện có thể sử dụng, False nếu không thỏa mãn
-- ****************************************************** --
function RuongKimPhong:OnPreCheckCondition(scene, item, player, otherParams)

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
function RuongKimPhong:OnUse(scene, item, player, otherParams)

    -- ************************** --
	local dialog = GUI.CreateItemDialog()
    dialog:AddText("Bạn có chắc chắn muốn sử dụng :" .. item:GetName() .. "?")

    dialog:AddSelection(1, "Ta đồng ý")
    dialog:AddSelection(2, "Ta suy nghĩ đã")

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
function RuongKimPhong:OnSelection(scene, item, player, selectionID, otherParams)

	-- ************************** --
	if selectionID == 2 then
		GUI.CloseDialog(player)
		return
	end
	-- ************************** --
	if selectionID == 1 then
		GUI.CloseDialog(player)
		-- Level rương
		--local itemLevel = item:GetItemLevel()
		-- Lượng NHHT nhận được
		--local nCount = tbLevel[itemLevel]
		
		-- Toác
	--Kiểu	if not tbLevel[itemLevel] then
			--self:ShowDialog(item, player, "Có lỗi khi sử dụng vật phẩm, hãy thử lại sau!")
		--	return
	--	end
		
		-- Túi không đủ chỗ trống
		if Player.HasFreeBagSpaces(player, 15) == false then
			self:ShowDialog(item, player, string.format("Túi đồ cần để trống tối thiểu <color=green>15 ô trống</color> trong túi đồ để lấy đồ Kim Phong!"))
			return
		end
		-- Xóa rương
       -- public static bool AddItemLua(Lua_Player player, int ItemID, int Number, int Series, int LockStatus, int enhanceLevel = 0, int ExpriesTime = -1)
       
		Player.RemoveItem(player, item:GetID())
		for key , value in pairs (ItemChess) do
			-- Thêm đồ Kim Phong tương 
			Player.AddItemLua(player, key, value.Number, value.Series ,value.Bound,value.enhanceLevel,value.Time)
		end
		self:ShowDialog(item, player, string.format("Nhận thành công <color=green>đồ Kim Phong!</color> "))
	end
	-- ************************** --
  
end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi chọn một trong các vật phẩm, và ấn nút Xác nhận cung cấp bởi vật phẩm thông qua Item Dialog
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
--		itemID: number - ID vật phẩm được chọn
-- ****************************************************** --
function RuongKimPhong:OnItemSelected(scene, item, player, itemID)

	-- ************************** --

	-- ************************** --

end

function RuongKimPhong:ShowDialog(item, player, text)

	-- ************************** --
	local dialog = GUI.CreateItemDialog()
	dialog:AddText(text)
	dialog:Show(item, player)
	-- ************************** --
	
end
