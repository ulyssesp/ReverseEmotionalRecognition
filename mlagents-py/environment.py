from mlagents_envs.environment import UnityEnvironment
from mlagents.trainers import learn
# This is a non-blocking call that only loads the environment.
# env = UnityEnvironment(file_name=None, seed=1, side_channels=[])
# Start interacting with the evironment.
# env.reset()
# behavior_names = env.behavior_spec.keys()

if __name__ == "__main__":
  opts = learn.parse_command_line(["../../ml-agents/config/trainer_config.yaml", "--run-id=testing_3", "--force"])
  print(opts)
  learn.run_training(1, opts)
