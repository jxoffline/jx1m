-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '200005' bên dưới thành ID tương ứng
local RuongKimQuang = Scripts[200111]

-- ************************** --
local ItemRuongKimQuang = {
    [1583] = {id= 1583, Time =-1 , Series=-1, Bound =1,enhanceLevel=0, Number=1 }
	,
	[1584] = {id= 1584, Time =-1 , Series=-1, Bound =1,enhanceLevel=0, Number=1 }
	,
	[1585] = {id= 1585, Time =-1 , Series=-1, Bound =1,enhanceLevel=0, Number=1 }
	,
	[1587] = {id= 1587, Time =-1 , Series=-1, Bound =1,enhanceLevel=0, Number=1 }
	,
	[1588] = {id= 1588, Time =-1 , Series=-1, Bound =1,enhanceLevel=0, Number=1 }
	,
	[1589] = {id= 1589, Time =-1 , Series=-1, Bound =1,enhanceLevel=0, Number=1 }
	,
	[1590] = {id= 1590, Time =-1 , Series=-1, Bound =1,enhanceLevel=0, Number=1 }
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
function RuongKimQuang:OnPreCheckCondition(scene, item, player, otherParams)

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
function RuongKimQuang:OnUse(scene, item, player, otherParams)

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
function RuongKimQuang:OnSelection(scene, item, player, selectionID, otherParams)

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
			self:ShowDialog(item, player, string.format("Túi đồ cần để trống tối thiểu <color=green>15 ô trống</color> trong túi đồ để lấy đồ Kim Quang!"))
			return
		end
		-- Xóa rương
       -- public static bool AddItemLua(Lua_Player player, int ItemID, int Number, int Series, int LockStatus, int enhanceLevel = 0, int ExpriesTime = -1)
       
		Player.RemoveItem(player, item:GetID())
		for key , value in pairs (ItemRuongKimQuang) do
			-- Thêm đồ Kim Quang tương 
			Player.AddItemLua(player, key, value.Number, value.Series ,value.Bound)
		end
		self:ShowDialog(item, player, string.format("Nhận thành công <color=green>Set Kim Quang!</color> "))
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
function RuongKimQuang:OnItemSelected(scene, item, player, itemID)

	-- ************************** --

	-- ************************** --

end

function RuongKimQuang:ShowDialog(item, player, text)

	-- ************************** --
	local dialog = GUI.CreateItemDialog()
	dialog:AddText(text)
	dialog:Show(item, player)
	-- ************************** --
	
end
