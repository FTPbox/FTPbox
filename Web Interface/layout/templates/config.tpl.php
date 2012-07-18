<script type="text/javascript">
	$(document).ready(function() {
		if($('#config_01_01').attr('checked')) {
			$('#config_01_03, #config_01_04').attr('readonly', false);
		}
		$('#config_01_01').click(function() {
			if($('#config_01_01').attr('checked')) {
				$('#config_01_03, #config_01_04').attr('readonly', false);
			} else {
				$('#config_01_03, #config_01_04').attr('readonly', true);
			}
		});
	});
</script>
<div id="config">
	<form action="index.php" method="POST">
		<input type="hidden" name="config" value="true" />
		<?php if(!empty($this->content['status'])): ?>
		<div id="config_status"><?php echo $this->content['status']; ?></div>
		<?php endif; ?>
		<!--<div id="config_01"> -->
			<input id="config_01_01" type="checkbox" name="locked" <?php if($this->content['locked'] === 'true'): ?>checked="checked"<?php endif; ?> />
			<label id="config_01_02" for="config_01_01"><?php echo $this->content['locked_label']; ?></label>
			<input id="config_01_03" type="password" value="" name="password" readonly />
			<input id="config_01_04" type="password" value="" name="password_repeat" readonly />
		<!-- </div>
		<div id="config_02">  -->
			<label id="config_02_01" for="config_02_02"><?php echo $this->content['root_label']; ?></label>
			<input id="config_02_02" type="text" value="<?php echo $this->content['root']; ?>" name="root" />
			<label id="config_02_03" for="config_02_04" class="button">Save<input id="config_02_04" type="submit" value="Save" /></label>
		<!-- </div> -->
	</form>
</div>
